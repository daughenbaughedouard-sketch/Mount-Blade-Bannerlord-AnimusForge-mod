using System.Collections.Generic;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace TaleWorlds.MountAndBlade.View.Scripts;

public class PopupSceneCameraPath : ScriptComponentBehavior
{
	public enum InterpolationType
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut
	}

	public struct PathAnimationState
	{
		public Path path;

		public string animationName;

		public float totalDistance;

		public float startTime;

		public float duration;

		public float alpha;

		public float easedAlpha;

		public bool fadeCamera;

		public InterpolationType interpolation;

		public string soundEvent;
	}

	public string LookAtEntity = "";

	public string SkeletonName = "";

	public int BoneIndex;

	public Vec3 AttachmentOffset = new Vec3(0f, 0f, 0f, -1f);

	public string InitialPath = "";

	public string InitialAnimationClip = "";

	public string InitialSound = "event:/mission/siege/siegetower/doorland";

	public float InitialPathStartTime;

	public float InitialPathDuration = 1f;

	public InterpolationType InitialInterpolation;

	public bool InitialFadeOut;

	public string PositivePath = "";

	public string PositiveAnimationClip = "";

	public string PositiveSound = "";

	public float PositivePathStartTime;

	public float PositivePathDuration = 1f;

	public InterpolationType PositiveInterpolation;

	public bool PositiveFadeOut;

	public string NegativePath = "";

	public string NegativeAnimationClip = "";

	public string NegativeSound = "";

	public float NegativePathStartTime;

	public float NegativePathDuration = 1f;

	public InterpolationType NegativeInterpolation;

	public bool NegativeFadeOut;

	private bool _isReady;

	public SimpleButton TestInitial;

	public SimpleButton TestPositive;

	public SimpleButton TestNegative;

	private MatrixFrame _localFrameIdentity = MatrixFrame.Identity;

	private GameEntity _lookAtEntity;

	private int _currentState;

	private float _cameraFadeValue;

	private List<PopupSceneSkeletonAnimationScript> _skeletonAnims = new List<PopupSceneSkeletonAnimationScript>();

	private SoundEvent _activeSoundEvent;

	private readonly PathAnimationState[] _transitionState = new PathAnimationState[3];

	protected override void OnInit()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((ScriptComponentBehavior)this).OnInit();
		((ScriptComponentBehavior)this).SetScriptComponentToTick(((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnEditorInit()
	{
		((ScriptComponentBehavior)this).OnEditorInit();
	}

	public void Initialize()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		if (!(SkeletonName != ""))
		{
			goto IL_005b;
		}
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if (!((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton == (NativeObject)null))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if (!(((WeakGameEntity)(ref gameEntity)).Skeleton.GetName() != SkeletonName))
			{
				goto IL_005b;
			}
		}
		GameEntityExtensions.CreateSimpleSkeleton(((ScriptComponentBehavior)this).GameEntity, SkeletonName);
		goto IL_0091;
		IL_005b:
		if (SkeletonName == "")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null)
			{
				gameEntity = ((ScriptComponentBehavior)this).GameEntity;
				((WeakGameEntity)(ref gameEntity)).RemoveSkeleton();
			}
		}
		goto IL_0091;
		IL_0091:
		if (LookAtEntity != "")
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			_lookAtEntity = ((WeakGameEntity)(ref gameEntity)).Scene.GetFirstEntityWithName(LookAtEntity);
		}
		ref PathAnimationState reference = ref _transitionState[0];
		object path;
		if (!(InitialPath == ""))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			path = ((WeakGameEntity)(ref gameEntity)).Scene.GetPathWithName(InitialPath);
		}
		else
		{
			path = null;
		}
		reference.path = (Path)path;
		_transitionState[0].animationName = InitialAnimationClip;
		_transitionState[0].startTime = InitialPathStartTime;
		_transitionState[0].duration = InitialPathDuration;
		_transitionState[0].interpolation = InitialInterpolation;
		_transitionState[0].fadeCamera = InitialFadeOut;
		_transitionState[0].soundEvent = InitialSound;
		ref PathAnimationState reference2 = ref _transitionState[1];
		object path2;
		if (!(PositivePath == ""))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			path2 = ((WeakGameEntity)(ref gameEntity)).Scene.GetPathWithName(PositivePath);
		}
		else
		{
			path2 = null;
		}
		reference2.path = (Path)path2;
		_transitionState[1].animationName = PositiveAnimationClip;
		_transitionState[1].startTime = PositivePathStartTime;
		_transitionState[1].duration = PositivePathDuration;
		_transitionState[1].interpolation = PositiveInterpolation;
		_transitionState[1].fadeCamera = PositiveFadeOut;
		_transitionState[1].soundEvent = PositiveSound;
		ref PathAnimationState reference3 = ref _transitionState[2];
		object path3;
		if (!(NegativePath == ""))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			path3 = ((WeakGameEntity)(ref gameEntity)).Scene.GetPathWithName(NegativePath);
		}
		else
		{
			path3 = null;
		}
		reference3.path = (Path)path3;
		_transitionState[2].animationName = NegativeAnimationClip;
		_transitionState[2].startTime = NegativePathStartTime;
		_transitionState[2].duration = NegativePathDuration;
		_transitionState[2].interpolation = NegativeInterpolation;
		_transitionState[2].fadeCamera = NegativeFadeOut;
		_transitionState[2].soundEvent = NegativeSound;
		MatrixFrame identity = MatrixFrame.Identity;
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		identity.origin = ((WeakGameEntity)(ref gameEntity)).GlobalPosition;
		SoundManager.SetListenerFrame(identity);
		List<GameEntity> list = new List<GameEntity>();
		((ScriptComponentBehavior)this).Scene.GetAllEntitiesWithScriptComponent<PopupSceneSkeletonAnimationScript>(ref list);
		list.ForEach(delegate(GameEntity e)
		{
			_skeletonAnims.Add(e.GetFirstScriptOfType<PopupSceneSkeletonAnimationScript>());
		});
		_skeletonAnims.ForEach(delegate(PopupSceneSkeletonAnimationScript s)
		{
			s.Initialize();
		});
	}

	private void SetState(int state)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null && !string.IsNullOrEmpty(_transitionState[state].animationName))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MBSkeletonExtensions.SetAnimationAtChannel(((WeakGameEntity)(ref gameEntity)).Skeleton, _transitionState[state].animationName, 0, 1f, -1f, 0f);
		}
		_currentState = state;
		_transitionState[state].alpha = 0f;
		if ((NativeObject)(object)_transitionState[state].path != (NativeObject)null)
		{
			_transitionState[state].totalDistance = _transitionState[state].path.GetTotalLength();
		}
		if (_transitionState[state].soundEvent != "")
		{
			SoundEvent activeSoundEvent = _activeSoundEvent;
			if (activeSoundEvent != null)
			{
				activeSoundEvent.Stop();
			}
			_activeSoundEvent = SoundEvent.CreateEventFromString(_transitionState[state].soundEvent, (Scene)null);
			if (_isReady)
			{
				SoundEvent activeSoundEvent2 = _activeSoundEvent;
				if (activeSoundEvent2 != null)
				{
					activeSoundEvent2.Play();
				}
			}
		}
		UpdateCamera(0f, ref _transitionState[state]);
		_skeletonAnims.ForEach(delegate(PopupSceneSkeletonAnimationScript s)
		{
			s.SetState(state);
		});
	}

	public void SetInitialState()
	{
		SetState(0);
	}

	public void SetPositiveState()
	{
		SetState(1);
	}

	public void SetNegativeState()
	{
		SetState(2);
	}

	public void SetIsReady(bool isReady)
	{
		if (_isReady == isReady)
		{
			return;
		}
		if (isReady)
		{
			SoundEvent activeSoundEvent = _activeSoundEvent;
			if (activeSoundEvent != null && !activeSoundEvent.IsPlaying())
			{
				_activeSoundEvent.Play();
			}
		}
		_isReady = isReady;
	}

	public float GetCameraFade()
	{
		return _cameraFadeValue;
	}

	public void Destroy()
	{
		SoundEvent activeSoundEvent = _activeSoundEvent;
		if (activeSoundEvent != null)
		{
			activeSoundEvent.Stop();
		}
		for (int i = 0; i < 3; i++)
		{
			_transitionState[i].path = null;
		}
	}

	private float InQuadBlend(float t)
	{
		return t * t;
	}

	private float OutQuadBlend(float t)
	{
		return t * (2f - t);
	}

	private float InOutQuadBlend(float t)
	{
		if (!(t < 0.5f))
		{
			return -1f + (4f - 2f * t) * t;
		}
		return 2f * t * t;
	}

	private MatrixFrame CreateLookAt(Vec3 position, Vec3 target, Vec3 upVector)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		Vec3 val = target - position;
		((Vec3)(ref val)).Normalize();
		Vec3 val2 = Vec3.CrossProduct(val, upVector);
		((Vec3)(ref val2)).Normalize();
		Vec3 val3 = Vec3.CrossProduct(val2, val);
		float x = val2.x;
		float y = val2.y;
		float z = val2.z;
		float num = 0f;
		float x2 = val3.x;
		float y2 = val3.y;
		float z2 = val3.z;
		float num2 = 0f;
		float num3 = 0f - val.x;
		float num4 = 0f - val.y;
		float num5 = 0f - val.z;
		float num6 = 0f;
		float x3 = position.x;
		float y3 = position.y;
		float z3 = position.z;
		float num7 = 1f;
		return new MatrixFrame(x, y, z, num, x2, y2, z2, num2, num3, num4, num5, num6, x3, y3, z3, num7);
	}

	private float Clamp(float x, float a, float b)
	{
		if (!(x < a))
		{
			if (!(x > b))
			{
				return x;
			}
			return b;
		}
		return a;
	}

	private float SmoothStep(float edge0, float edge1, float x)
	{
		x = Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
		return x * x * (3f - 2f * x);
	}

	private void UpdateCamera(float dt, ref PathAnimationState state)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		WeakGameEntity gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		GameEntity val = ((WeakGameEntity)(ref gameEntity)).Scene.FindEntityWithTag("camera_instance");
		if (val == (GameEntity)null)
		{
			return;
		}
		state.alpha += dt;
		if (state.alpha > state.startTime + state.duration)
		{
			state.alpha = state.startTime + state.duration;
		}
		float num = SmoothStep(state.startTime, state.startTime + state.duration, state.alpha);
		switch (state.interpolation)
		{
		case InterpolationType.EaseIn:
			num = InQuadBlend(num);
			break;
		case InterpolationType.EaseOut:
			num = OutQuadBlend(num);
			break;
		case InterpolationType.EaseInOut:
			num = InOutQuadBlend(num);
			break;
		}
		state.easedAlpha = num;
		if (state.fadeCamera)
		{
			_cameraFadeValue = num;
		}
		gameEntity = ((ScriptComponentBehavior)this).GameEntity;
		if ((NativeObject)(object)((WeakGameEntity)(ref gameEntity)).Skeleton != (NativeObject)null && !string.IsNullOrEmpty(state.animationName))
		{
			gameEntity = ((ScriptComponentBehavior)this).GameEntity;
			MatrixFrame boneEntitialFrame = ((WeakGameEntity)(ref gameEntity)).Skeleton.GetBoneEntitialFrame((sbyte)BoneIndex);
			boneEntitialFrame = ((MatrixFrame)(ref _localFrameIdentity)).TransformToParent(ref boneEntitialFrame);
			MatrixFrame listenerFrame = new MatrixFrame
			{
				rotation = boneEntitialFrame.rotation,
				rotation = 
				{
					u = -boneEntitialFrame.rotation.s
				},
				rotation = 
				{
					f = -boneEntitialFrame.rotation.u
				},
				rotation = 
				{
					s = boneEntitialFrame.rotation.f
				},
				origin = boneEntitialFrame.origin + AttachmentOffset
			};
			val.SetFrame(ref listenerFrame, true);
			SoundManager.SetListenerFrame(listenerFrame);
		}
		else if ((NativeObject)(object)state.path != (NativeObject)null)
		{
			float num2 = num * state.totalDistance;
			Vec3 origin = state.path.GetFrameForDistance(num2).origin;
			MatrixFrame val2 = val.GetGlobalFrame();
			if (_lookAtEntity != (GameEntity)null)
			{
				val2 = CreateLookAt(origin, _lookAtEntity.GetGlobalFrame().origin, Vec3.Up);
			}
			else
			{
				val2.origin = origin;
			}
			val.SetGlobalFrame(ref val2, true);
		}
	}

	public override TickRequirement GetTickRequirement()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (TickRequirement)(2 | ((ScriptComponentBehavior)this).GetTickRequirement());
	}

	protected override void OnTick(float dt)
	{
		UpdateCamera(dt, ref _transitionState[_currentState]);
	}

	protected override void OnEditorTick(float dt)
	{
		((ScriptComponentBehavior)this).OnEditorTick(dt);
		((ScriptComponentBehavior)this).OnTick(dt);
	}

	protected override void OnEditorVariableChanged(string variableName)
	{
		((ScriptComponentBehavior)this).OnEditorVariableChanged(variableName);
		Initialize();
		if (variableName == "TestInitial")
		{
			SetState(0);
		}
		if (variableName == "TestPositive")
		{
			SetState(1);
		}
		if (variableName == "TestNegative")
		{
			SetState(2);
		}
	}
}
