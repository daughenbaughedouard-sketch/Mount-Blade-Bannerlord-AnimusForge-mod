param(
	[string]$RepoRoot = "",
	[string]$OldSourceRoot = "",
	[string]$BannerlordRoot = "F:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord",
	[string]$OutputPath = ""
)

Set-StrictMode -Version 3
$ErrorActionPreference = "Stop"

function Get-FullPath {
	param([string]$Path)
	if ([string]::IsNullOrWhiteSpace($Path)) {
		return $Path
	}
	return [System.IO.Path]::GetFullPath($Path)
}

function Find-OldSourceRoot {
	param([string]$Root)
	$candidates = Get-ChildItem -LiteralPath $Root -Directory | Where-Object {
		Test-Path -LiteralPath (Join-Path $_.FullName "TaleWorlds.CampaignSystem") -PathType Container
	}
	return $candidates | Select-Object -First 1 -ExpandProperty FullName
}

function Normalize-PathForRegex {
	param([string]$Path)
	return [regex]::Escape((Get-FullPath $Path).TrimEnd('\'))
}

function Write-Section {
	param(
		[System.Text.StringBuilder]$Builder,
		[string]$Title
	)
	[void]$Builder.AppendLine("")
	[void]$Builder.AppendLine("## $Title")
}

function Get-SafeTypesFromAssembly {
	param([System.Reflection.Assembly]$Assembly)
	try {
		return $Assembly.GetTypes()
	}
	catch [System.Reflection.ReflectionTypeLoadException] {
		return $_.Exception.Types | Where-Object { $_ -ne $null }
	}
	catch {
		return @()
	}
}

function Get-AssemblyPath {
	param(
		[string]$BannerlordRoot,
		[string]$AssemblyName
	)
	$primary = Join-Path $BannerlordRoot ("bin\Win64_Shipping_Client\" + $AssemblyName)
	if (Test-Path -LiteralPath $primary -PathType Leaf) {
		return $primary
	}
	$moduleCandidates = Get-ChildItem -LiteralPath (Join-Path $BannerlordRoot "Modules") -Directory -ErrorAction SilentlyContinue
	foreach ($module in $moduleCandidates) {
		$candidate = Join-Path $module.FullName ("bin\Win64_Shipping_Client\" + $AssemblyName)
		if (Test-Path -LiteralPath $candidate -PathType Leaf) {
			return $candidate
		}
	}
	return ""
}

function Load-GameAssemblies {
	param([string]$BannerlordRoot)
	$assemblyNames = @(
		"TaleWorlds.Library.dll",
		"TaleWorlds.Localization.dll",
		"TaleWorlds.ObjectSystem.dll",
		"TaleWorlds.Engine.dll",
		"TaleWorlds.InputSystem.dll",
		"TaleWorlds.ScreenSystem.dll",
		"TaleWorlds.GauntletUI.dll",
		"TaleWorlds.GauntletUI.Native.dll",
		"TaleWorlds.Core.dll",
		"TaleWorlds.CampaignSystem.dll",
		"TaleWorlds.MountAndBlade.dll",
		"TaleWorlds.MountAndBlade.GauntletUI.dll",
		"TaleWorlds.MountAndBlade.View.dll",
		"SandBox.dll",
		"SandBox.View.dll",
		"StoryMode.dll",
		"StoryMode.View.dll"
	)
	$loaded = @()
	foreach ($name in $assemblyNames) {
		$path = Get-AssemblyPath -BannerlordRoot $BannerlordRoot -AssemblyName $name
		if ([string]::IsNullOrWhiteSpace($path)) {
			continue
		}
		try {
			$loaded += [System.Reflection.Assembly]::LoadFrom($path)
		}
		catch {
		}
	}
	return $loaded
}

function Add-IndexEntry {
	param(
		[hashtable]$Index,
		[string]$Key,
		[object]$Value
	)
	if (-not $Index.ContainsKey($Key)) {
		$Index[$Key] = [System.Collections.Generic.List[object]]::new()
	}
	$Index[$Key].Add($Value)
}

function Get-IndexValues {
	param(
		[hashtable]$Index,
		[string]$Key
	)
	if ($Index.ContainsKey($Key)) {
		return $Index[$Key]
	}
	return @()
}

function Build-OldTypeIndex {
	param([string]$SourceRoot)
	$index = @{}
	$files = Get-ChildItem -LiteralPath $SourceRoot -Recurse -File -Filter *.cs
	foreach ($file in $files) {
		$content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
		$namespace = ""
		$nsMatch = [regex]::Match($content, '(?m)^\s*namespace\s+([A-Za-z0-9_\.]+)')
		if ($nsMatch.Success) {
			$namespace = $nsMatch.Groups[1].Value
		}
		$typeMatches = [regex]::Matches($content, '(?m)^\s*(?:public|internal|private|protected)?\s*(?:static\s+|abstract\s+|sealed\s+|partial\s+|unsafe\s+|new\s+)*?(class|struct|interface|enum)\s+([A-Za-z_][A-Za-z0-9_]*)')
		foreach ($match in $typeMatches) {
			$typeName = $match.Groups[2].Value
			if ([string]::IsNullOrWhiteSpace($typeName)) {
				continue
			}
			$fullName = if ([string]::IsNullOrWhiteSpace($namespace)) { $typeName } else { "$namespace.$typeName" }
			$entry = [pscustomobject]@{
				FullName = $fullName
				FilePath = $file.FullName
				Namespace = $namespace
			}
			Add-IndexEntry -Index $index -Key $typeName -Value $entry
			Add-IndexEntry -Index $index -Key $fullName -Value $entry
		}
	}
	return $index
}

function Build-CurrentTypeIndex {
	param([System.Reflection.Assembly[]]$Assemblies)
	$index = @{}
	foreach ($assembly in $Assemblies) {
		foreach ($type in (Get-SafeTypesFromAssembly $assembly)) {
			if ($type -eq $null) {
				continue
			}
			if ([string]::IsNullOrWhiteSpace($type.Name) -or [string]::IsNullOrWhiteSpace($type.FullName)) {
				continue
			}
			if ($type.FullName -notmatch '^(TaleWorlds|SandBox|StoryMode)\.') {
				continue
			}
			$entry = [pscustomobject]@{
				FullName = $type.FullName
				Type = $type
				Assembly = $assembly.GetName().Name
			}
			Add-IndexEntry -Index $index -Key $type.Name -Value $entry
			Add-IndexEntry -Index $index -Key $type.FullName -Value $entry
		}
	}
	return $index
}

function Find-CurrentTypeByFullName {
	param(
		[System.Reflection.Assembly[]]$Assemblies,
		[string]$FullName
	)
	foreach ($assembly in $Assemblies) {
		try {
			$type = $assembly.GetType($FullName, $false, $false)
			if ($type -ne $null) {
				return [pscustomobject]@{
					FullName = $type.FullName
					Type = $type
					Assembly = $assembly.GetName().Name
				}
			}
		}
		catch {
		}
	}
	return $null
}

function Get-PathLineNumber {
	param(
		[string]$Text,
		[int]$Index
	)
	if ($Index -le 0) {
		return 1
	}
	$prefix = $Text.Substring(0, $Index)
	return ([regex]::Matches($prefix, "`n").Count + 1)
}

function Add-Reference {
	param(
		[System.Collections.Generic.List[object]]$References,
		[string]$Kind,
		[string]$TypeToken,
		[string]$MemberName,
		[string]$FilePath,
		[int]$LineNumber,
		[string]$FullNameHint
	)
	$References.Add([pscustomobject]@{
		Kind = $Kind
		TypeToken = $TypeToken
		MemberName = $MemberName
		FilePath = $FilePath
		LineNumber = $LineNumber
		FullNameHint = $FullNameHint
	})
}

function Build-ModReferences {
	param(
		[string]$Root,
		[string]$OldSourceRoot
	)
	$references = [System.Collections.Generic.List[object]]::new()
	$oldSourceRegex = Normalize-PathForRegex $OldSourceRoot
	$files = Get-ChildItem -LiteralPath $Root -Recurse -File -Filter *.cs | Where-Object {
		$full = $_.FullName
		return ($full -notmatch '\\bin\\') -and ($full -notmatch '\\obj\\') -and ($full -notmatch $oldSourceRegex)
	}
	$patterns = @(
		@{
			Regex = '\[HarmonyPatch\(typeof\((?<type>[^)]+)\),\s*"(?<member>[^"]+)"'
			Kind = 'method'
		},
		@{
			Regex = 'AccessTools\.(?<kind>Method|Field|Property)\(typeof\((?<type>[^)]+)\),\s*"(?<member>[^"]+)"'
			Kind = 'dynamic'
		},
		@{
			Regex = 'AccessTools\.TypeByName\("(?<full>[^"]+)"\)'
			Kind = 'type'
		},
		@{
			Regex = 'GetMissionBehavior<(?<type>[A-Za-z0-9_\.]+)>'
			Kind = 'type'
		}
	)
	foreach ($file in $files) {
		$content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
		foreach ($pattern in $patterns) {
			$matches = [regex]::Matches($content, $pattern.Regex, [System.Text.RegularExpressions.RegexOptions]::Singleline)
			foreach ($match in $matches) {
				$line = Get-PathLineNumber -Text $content -Index $match.Index
				switch ($pattern.Kind) {
					'dynamic' {
						$kind = $match.Groups['kind'].Value.ToLowerInvariant()
						Add-Reference -References $references -Kind $kind -TypeToken $match.Groups['type'].Value.Trim() -MemberName $match.Groups['member'].Value.Trim() -FilePath $file.FullName -LineNumber $line -FullNameHint ""
					}
					'method' {
						Add-Reference -References $references -Kind 'method' -TypeToken $match.Groups['type'].Value.Trim() -MemberName $match.Groups['member'].Value.Trim() -FilePath $file.FullName -LineNumber $line -FullNameHint ""
					}
					'type' {
						if ($match.Groups['full'].Success) {
							$full = $match.Groups['full'].Value.Trim()
							$typeToken = $full.Split('.')[-1]
							Add-Reference -References $references -Kind 'type' -TypeToken $typeToken -MemberName "" -FilePath $file.FullName -LineNumber $line -FullNameHint $full
						}
						else {
							Add-Reference -References $references -Kind 'type' -TypeToken $match.Groups['type'].Value.Trim() -MemberName "" -FilePath $file.FullName -LineNumber $line -FullNameHint ""
						}
					}
				}
			}
		}
	}
	return $references
}

function Resolve-TypeReference {
	param(
		[pscustomobject]$Reference,
		[hashtable]$OldTypeIndex,
		[hashtable]$CurrentTypeIndex,
		[System.Reflection.Assembly[]]$Assemblies
	)
	$resolvedCurrent = @()
	$resolvedOld = @()
	if (-not [string]::IsNullOrWhiteSpace($Reference.FullNameHint)) {
		$resolvedCurrent += Get-IndexValues -Index $CurrentTypeIndex -Key $Reference.FullNameHint
		$resolvedOld += Get-IndexValues -Index $OldTypeIndex -Key $Reference.FullNameHint
		if ($resolvedCurrent.Count -eq 0) {
			$directCurrent = Find-CurrentTypeByFullName -Assemblies $Assemblies -FullName $Reference.FullNameHint
			if ($directCurrent -ne $null) {
				$resolvedCurrent += $directCurrent
			}
		}
	}
	if ($resolvedCurrent.Count -eq 0 -and $CurrentTypeIndex.ContainsKey($Reference.TypeToken)) {
		$resolvedCurrent += $CurrentTypeIndex[$Reference.TypeToken]
	}
	if ($resolvedOld.Count -eq 0 -and $OldTypeIndex.ContainsKey($Reference.TypeToken)) {
		$resolvedOld += $OldTypeIndex[$Reference.TypeToken]
	}
	$chosenCurrent = $null
	$chosenOld = $null
	if ($resolvedCurrent.Count -eq 1) {
		$chosenCurrent = $resolvedCurrent[0]
	}
	if ($resolvedOld.Count -eq 1) {
		$chosenOld = $resolvedOld[0]
	}
	if ($chosenCurrent -eq $null -and $chosenOld -ne $null) {
		$chosenCurrent = $resolvedCurrent | Where-Object { $_.FullName -eq $chosenOld.FullName } | Select-Object -First 1
	}
	if ($chosenOld -eq $null -and $chosenCurrent -ne $null) {
		$chosenOld = $resolvedOld | Where-Object { $_.FullName -eq $chosenCurrent.FullName } | Select-Object -First 1
	}
	if ($chosenCurrent -eq $null -and $resolvedCurrent.Count -gt 0) {
		$chosenCurrent = $resolvedCurrent | Where-Object { $_.FullName -match '^(TaleWorlds|SandBox|StoryMode)\.' } | Select-Object -First 1
	}
	if ($chosenOld -eq $null -and $resolvedOld.Count -gt 0) {
		$chosenOld = $resolvedOld | Where-Object { $_.FullName -match '^(TaleWorlds|SandBox|StoryMode)\.' } | Select-Object -First 1
	}
	return [pscustomobject]@{
		Reference = $Reference
		Current = $chosenCurrent
		Old = $chosenOld
		CurrentCandidates = $resolvedCurrent
		OldCandidates = $resolvedOld
	}
}

function Get-CurrentMemberSets {
	param([Type]$Type)
	$flags = [System.Reflection.BindingFlags]"Public, NonPublic, Instance, Static, DeclaredOnly"
	$methods = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	$properties = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	$fields = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	foreach ($method in $Type.GetMethods($flags)) {
		if ($method.IsSpecialName) {
			continue
		}
		if ($method.Name.StartsWith("AutoGenerated")) {
			continue
		}
		[void]$methods.Add($method.Name)
	}
	foreach ($property in $Type.GetProperties($flags)) {
		if ($property.IsSpecialName) {
			continue
		}
		[void]$properties.Add($property.Name)
	}
	foreach ($field in $Type.GetFields($flags)) {
		if ($field.IsSpecialName) {
			continue
		}
		if ($field.Name.StartsWith("<")) {
			continue
		}
		[void]$fields.Add($field.Name)
	}
	return [pscustomobject]@{
		Methods = $methods
		Properties = $properties
		Fields = $fields
	}
}

function Get-OldMemberSets {
	param(
		[string]$FilePath,
		[string]$TypeName
	)
	$methods = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	$properties = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	$fields = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::Ordinal)
	$lines = Get-Content -LiteralPath $FilePath -Encoding UTF8
	foreach ($line in $lines) {
		$trimmed = $line.Trim()
		if ($trimmed.StartsWith("//")) {
			continue
		}
		$propertyMatch = [regex]::Match($trimmed, '^(?:public|private|internal|protected)\s+(?:static\s+|virtual\s+|override\s+|sealed\s+|abstract\s+|unsafe\s+|new\s+)*[A-Za-z0-9_<>\.\[\],`\?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\{\s*(?:get|set)')
		if ($propertyMatch.Success) {
			[void]$properties.Add($propertyMatch.Groups[1].Value)
			continue
		}
		$methodMatch = [regex]::Match($trimmed, '^(?:public|private|internal|protected)\s+(?:static\s+|virtual\s+|override\s+|sealed\s+|abstract\s+|unsafe\s+|new\s+)*[A-Za-z0-9_<>\.\[\],`\?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(')
		if ($methodMatch.Success) {
			$name = $methodMatch.Groups[1].Value
			if ($name -ne $TypeName -and -not $name.StartsWith("AutoGenerated")) {
				[void]$methods.Add($name)
			}
			continue
		}
		$fieldMatch = [regex]::Match($trimmed, '^(?:public|private|internal|protected)\s+(?:static\s+|readonly\s+|volatile\s+|unsafe\s+|new\s+)*[A-Za-z0-9_<>\.\[\],`\?]+\s+([A-Za-z_][A-Za-z0-9_]*)\s*(?:=|;)')
		if ($fieldMatch.Success) {
			$name = $fieldMatch.Groups[1].Value
			if (-not $name.StartsWith("<")) {
				[void]$fields.Add($name)
			}
		}
	}
	return [pscustomobject]@{
		Methods = $methods
		Properties = $properties
		Fields = $fields
	}
}

function Test-MemberExists {
	param(
		[pscustomobject]$MemberSets,
		[string]$Kind,
		[string]$MemberName
	)
	switch ($Kind) {
		'method' { return $MemberSets.Methods.Contains($MemberName) }
		'property' { return $MemberSets.Properties.Contains($MemberName) }
		'field' { return $MemberSets.Fields.Contains($MemberName) }
		default { return $true }
	}
}

function Join-Names {
	param([System.Collections.IEnumerable]$Values)
	$sorted = $Values | Sort-Object | Select-Object -First 12
	return ($sorted -join ', ')
}

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
	$RepoRoot = Get-FullPath (Join-Path $PSScriptRoot "..")
}
else {
	$RepoRoot = Get-FullPath $RepoRoot
}

if ([string]::IsNullOrWhiteSpace($OldSourceRoot)) {
	$OldSourceRoot = Find-OldSourceRoot -Root $RepoRoot
}
else {
	$OldSourceRoot = Get-FullPath $OldSourceRoot
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
	$OutputPath = Join-Path $RepoRoot "CompatReports\NativeApiDiff.txt"
}
else {
	$OutputPath = Get-FullPath $OutputPath
}

$binRoot = Join-Path $BannerlordRoot "bin\Win64_Shipping_Client"
if (-not (Test-Path -LiteralPath $OldSourceRoot -PathType Container)) {
	throw "Old source root not found: $OldSourceRoot"
}
if (-not (Test-Path -LiteralPath $binRoot -PathType Container)) {
	throw "Bannerlord bin root not found: $binRoot"
}

$assemblies = Load-GameAssemblies -BannerlordRoot $BannerlordRoot
$oldTypeIndex = Build-OldTypeIndex -SourceRoot $OldSourceRoot
$currentTypeIndex = Build-CurrentTypeIndex -Assemblies $assemblies
$references = Build-ModReferences -Root $RepoRoot -OldSourceRoot $OldSourceRoot
$resolved = foreach ($reference in $references) {
	Resolve-TypeReference -Reference $reference -OldTypeIndex $oldTypeIndex -CurrentTypeIndex $currentTypeIndex -Assemblies $assemblies
}

$builder = [System.Text.StringBuilder]::new()
[void]$builder.AppendLine("AnimusForge Native API Diff Report")
[void]$builder.AppendLine("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
[void]$builder.AppendLine("RepoRoot: $RepoRoot")
[void]$builder.AppendLine("OldSourceRoot: $OldSourceRoot")
[void]$builder.AppendLine("BannerlordBin: $binRoot")

Write-Section -Builder $builder -Title "Summary"
$resolvedTypeGroups = $resolved | Group-Object { if ($_.Current -ne $null) { $_.Current.FullName } elseif ($_.Old -ne $null) { $_.Old.FullName } elseif (-not [string]::IsNullOrWhiteSpace($_.Reference.FullNameHint)) { $_.Reference.FullNameHint } else { $_.Reference.TypeToken } }
$missingTypes = $resolved | Where-Object {
	if ($_.Reference.Kind -ne 'type' -or $_.Current -ne $null) {
		return $false
	}
	if (-not [string]::IsNullOrWhiteSpace($_.Reference.FullNameHint)) {
		return $_.Reference.FullNameHint -match '^(TaleWorlds|SandBox|StoryMode)\.'
	}
	if ($_.Old -ne $null) {
		return $_.Old.FullName -match '^(TaleWorlds|SandBox|StoryMode)\.'
	}
	return $false
}
$unresolvedMembers = [System.Collections.Generic.List[object]]::new()
$typeDiffRows = [System.Collections.Generic.List[object]]::new()

$groupedByType = $resolved | Group-Object { if ($_.Current -ne $null) { $_.Current.FullName } elseif ($_.Old -ne $null) { $_.Old.FullName } elseif (-not [string]::IsNullOrWhiteSpace($_.Reference.FullNameHint)) { $_.Reference.FullNameHint } else { $_.Reference.TypeToken } }
foreach ($group in $groupedByType) {
	$sample = $group.Group[0]
	if ($group.Name -notmatch '^(TaleWorlds|SandBox|StoryMode)\.') {
		continue
	}
	$currentType = if ($sample.Current -ne $null) { $sample.Current.Type } else { $null }
	$currentMemberSets = if ($currentType -ne $null) { Get-CurrentMemberSets -Type $currentType } else { $null }
	$oldMemberSets = $null
	$oldFilePath = $null
	$typeNameForOld = ""
	if ($sample.Old -ne $null) {
		$oldFilePath = $sample.Old.FilePath
		$typeNameForOld = $sample.Old.FullName.Split('.')[-1]
		$oldMemberSets = Get-OldMemberSets -FilePath $oldFilePath -TypeName $typeNameForOld
	}
	foreach ($item in $group.Group) {
		if ($item.Reference.Kind -in @('method', 'property', 'field')) {
			if ($currentMemberSets -eq $null -or -not (Test-MemberExists -MemberSets $currentMemberSets -Kind $item.Reference.Kind -MemberName $item.Reference.MemberName)) {
				$unresolvedMembers.Add([pscustomobject]@{
					TypeName = $group.Name
					Kind = $item.Reference.Kind
					MemberName = $item.Reference.MemberName
					FilePath = $item.Reference.FilePath
					LineNumber = $item.Reference.LineNumber
				})
			}
		}
	}
	if ($currentMemberSets -ne $null -and $oldMemberSets -ne $null) {
		$oldOnlyMethods = $oldMemberSets.Methods.Where({ -not $currentMemberSets.Methods.Contains($_) })
		$currentOnlyMethods = $currentMemberSets.Methods.Where({ -not $oldMemberSets.Methods.Contains($_) })
		$oldOnlyProperties = $oldMemberSets.Properties.Where({ -not $currentMemberSets.Properties.Contains($_) })
		$currentOnlyProperties = $currentMemberSets.Properties.Where({ -not $oldMemberSets.Properties.Contains($_) })
		$oldOnlyFields = $oldMemberSets.Fields.Where({ -not $currentMemberSets.Fields.Contains($_) })
		$currentOnlyFields = $currentMemberSets.Fields.Where({ -not $oldMemberSets.Fields.Contains($_) })
		$typeDiffRows.Add([pscustomobject]@{
			TypeName = $group.Name
			OldFilePath = $oldFilePath
			ReferencedCount = $group.Count
			OldOnlyMethodCount = @($oldOnlyMethods).Count
			CurrentOnlyMethodCount = @($currentOnlyMethods).Count
			OldOnlyPropertyCount = @($oldOnlyProperties).Count
			CurrentOnlyPropertyCount = @($currentOnlyProperties).Count
			OldOnlyFieldCount = @($oldOnlyFields).Count
			CurrentOnlyFieldCount = @($currentOnlyFields).Count
			OldOnlyMethods = Join-Names $oldOnlyMethods
			CurrentOnlyMethods = Join-Names $currentOnlyMethods
			OldOnlyProperties = Join-Names $oldOnlyProperties
			CurrentOnlyProperties = Join-Names $currentOnlyProperties
			OldOnlyFields = Join-Names $oldOnlyFields
			CurrentOnlyFields = Join-Names $currentOnlyFields
		})
	}
}

[void]$builder.AppendLine("Scanned references: $($references.Count)")
[void]$builder.AppendLine("Resolved native type groups: $($resolvedTypeGroups.Count)")
[void]$builder.AppendLine("Missing referenced types in current DLLs: $($missingTypes.Count)")
[void]$builder.AppendLine("Missing referenced members in current DLLs: $($unresolvedMembers.Count)")

Write-Section -Builder $builder -Title "Missing Referenced Types"
if ($missingTypes.Count -eq 0) {
	[void]$builder.AppendLine("None")
}
else {
	foreach ($row in ($missingTypes | Sort-Object { $_.Reference.FullNameHint }, { $_.Reference.TypeToken } | Select-Object -First 40)) {
		$typeName = if (-not [string]::IsNullOrWhiteSpace($row.Reference.FullNameHint)) { $row.Reference.FullNameHint } else { $row.Reference.TypeToken }
		[void]$builder.AppendLine("- $typeName")
	}
}

Write-Section -Builder $builder -Title "Missing Referenced Members"
if ($unresolvedMembers.Count -eq 0) {
	[void]$builder.AppendLine("None")
}
else {
	foreach ($row in ($unresolvedMembers | Sort-Object TypeName, Kind, MemberName | Select-Object -First 80)) {
		$relPath = Resolve-Path -LiteralPath $row.FilePath -Relative
		[void]$builder.AppendLine(("- {0} :: {1} {2} <- {3}:{4}" -f $row.TypeName, $row.Kind, $row.MemberName, $relPath, $row.LineNumber))
	}
}

Write-Section -Builder $builder -Title "Top Type Deltas"
$topRows = $typeDiffRows | Sort-Object `
	@{Expression = { $_.OldOnlyMethodCount + $_.CurrentOnlyMethodCount + $_.OldOnlyPropertyCount + $_.CurrentOnlyPropertyCount + $_.OldOnlyFieldCount + $_.CurrentOnlyFieldCount }; Descending = $true}, `
	@{Expression = { $_.ReferencedCount }; Descending = $true} | Select-Object -First 25
if ($topRows.Count -eq 0) {
	[void]$builder.AppendLine("None")
}
else {
	foreach ($row in $topRows) {
		$totalDelta = $row.OldOnlyMethodCount + $row.CurrentOnlyMethodCount + $row.OldOnlyPropertyCount + $row.CurrentOnlyPropertyCount + $row.OldOnlyFieldCount + $row.CurrentOnlyFieldCount
		[void]$builder.AppendLine("- $($row.TypeName) | refs=$($row.ReferencedCount) delta=$totalDelta")
		if (-not [string]::IsNullOrWhiteSpace($row.OldOnlyMethods)) {
			[void]$builder.AppendLine("  old-only methods: $($row.OldOnlyMethods)")
		}
		if (-not [string]::IsNullOrWhiteSpace($row.CurrentOnlyMethods)) {
			[void]$builder.AppendLine("  current-only methods: $($row.CurrentOnlyMethods)")
		}
		if (-not [string]::IsNullOrWhiteSpace($row.OldOnlyProperties)) {
			[void]$builder.AppendLine("  old-only properties: $($row.OldOnlyProperties)")
		}
		if (-not [string]::IsNullOrWhiteSpace($row.CurrentOnlyProperties)) {
			[void]$builder.AppendLine("  current-only properties: $($row.CurrentOnlyProperties)")
		}
		if (-not [string]::IsNullOrWhiteSpace($row.OldOnlyFields)) {
			[void]$builder.AppendLine("  old-only fields: $($row.OldOnlyFields)")
		}
		if (-not [string]::IsNullOrWhiteSpace($row.CurrentOnlyFields)) {
			[void]$builder.AppendLine("  current-only fields: $($row.CurrentOnlyFields)")
		}
	}
}

$outputDir = Split-Path $OutputPath -Parent
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
[System.IO.File]::WriteAllText($OutputPath, $builder.ToString(), [System.Text.Encoding]::UTF8)
Write-Host "Native API diff report written to $OutputPath"
