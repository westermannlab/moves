using UnityEngine;

public class Player : ScriptableObject
{
   public string PlayerName;
   public Personality Personality;
   public string Identifier;
   public Color Color;
   public Color SoftColor;
   public Soul SoulPrefab;
   public Gradient SoulGradient;
   public int PlayerId;
   public int ColorId;
   public Vector2Int BlendOffset;
   public SoundEffect Music;

   public Player Counterpart;
   public ControllableObject.Type CurrentType;
   public bool CanTransferSoul;

   public string AssessmentCaption;
   public string AssessmentColor;

   private Soul _soul;
   private ControllableObject _currentObject;

   private Pixel _currentTransferPixel;
   private const float TransferSoulDuration = 1f;
   private const int TransferPixelsPerSecond = 100;
   private int _transferPixelCount;
   private float _transferSoulTimer;
   private bool _isTransferActive;

   private Vector2 _bezierControlPoint;
   private float _beginAnimateTime;
   private float _lastUpdateTime;

   private bool _isVisible;

   private SerializableDictionary<ControllableObject.Type, float> _timeSpentInObjectDictionary = new SerializableDictionary<ControllableObject.Type, float>();
   private SerializableDictionary<ControllableObject.Type, int> _objectEnterCountDictionary = new SerializableDictionary<ControllableObject.Type, int>();

   public Soul Soul => _soul;

   public void InitSoul()
   {
      if (SoulPrefab)
      {
         _soul = Instantiate(SoulPrefab, Vector2.up * 2f, Quaternion.identity);
         _soul.name = PlayerName + " Soul";
         _soul.Enter(this);
         
         if (Counterpart != null && Counterpart.ColorId == 0)
         {
            _soul.MoveToPosition(new Vector2(0.625f, 2f));
         }
         else if (Counterpart != null && Counterpart.ColorId < 6)
         {
            _soul.MoveToPosition(new Vector2(-0.625f, 2f));
         }
      }

      // initialize after entering soul
      InitializeDictionaries();

      if (Personality != null)
      {
         Personality.LoadPersonalityFromData(this);
      }

      _isVisible = true;
      CanTransferSoul = true;
   }

   public void TakeControl()
   {
      Controllers.Input.ControllablePlayer = this;
      SetAsTarget();
      UpdateKeyFunctions();
   }

   private void SetAsTarget()
   {
      Controllers.Camera.MainCamera.SetTarget(_currentObject);
   }
   
   public void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
   {
      if (_currentObject != null)
      {
         if (inputMode == InputController.Mode.Release)
         {
            CanTransferSoul = true;
         }
         _currentObject.ProcessInput(inputType, inputMode, x, y);
      }

      switch (inputType)
      {
         case InputController.Type.Space:
            var trigger = _soul.SoulTrigger;
            if (inputMode == InputController.Mode.Hold)
            {
               if (_currentObject != null && _currentObject.ObjectType == ControllableObject.Type.Soul)
               {
                  if (trigger != null)
                  {
                     TransferSoulTo(trigger.ControllableObject);
                  }
               }
            }
            else if (inputMode == InputController.Mode.Release)
            {
               CancelSoulTransfer(trigger != null ? trigger.ControllableObject.ObjectType : ControllableObject.Type.None);
            }
            break;
      }
   }

   public void Enter(ControllableObject controllableObject)
   {
      UpdateTimeSpentInObjects();

      _currentObject = controllableObject;
      CurrentType = controllableObject.ObjectType;
      if (Controllers.Input.ControllablePlayer == this || Counterpart.CurrentType == ControllableObject.Type.Ground)
      {
         if (CurrentType != ControllableObject.Type.Ground)
         {
            SetAsTarget();
         }
         else
         {
            Counterpart.SetAsTarget();
         }
      }
      
      if (!_objectEnterCountDictionary.ContainsKey(CurrentType))
      {
         _objectEnterCountDictionary.Add(CurrentType, 0);
      }
      _objectEnterCountDictionary[CurrentType]++;

      // disable soul transfer until a button is released
      CanTransferSoul = false;

      if (Controllers.Input.ControllablePlayer == this)
      {
         UpdateKeyFunctions();
      }

      if (Personality != null)
      {
         Personality.WhenRolesHaveChanged();
      }

      if (Counterpart != null && Counterpart.Personality != null)
      {
         Counterpart.Personality.WhenRolesHaveChanged();
      }
   }

   public void UpdateTimeSpentInObjects()
   {
      if (!_timeSpentInObjectDictionary.ContainsKey(CurrentType))
      {
         _timeSpentInObjectDictionary.Add(CurrentType, 0f);
      }

      _timeSpentInObjectDictionary[CurrentType] += References.Timer.GetLevelTime() - _lastUpdateTime;
      
      _lastUpdateTime = References.Timer.GetLevelTime();
   }

   public float GetTimeSpentInObject(ControllableObject.Type type)
   {
      if (!_timeSpentInObjectDictionary.ContainsKey(type))
      {
         return -1f;
      }

      return _timeSpentInObjectDictionary[type];
   }
   
   public float GetObjectEnterCount(ControllableObject.Type type)
   {
      if (!_objectEnterCountDictionary.ContainsKey(type))
      {
         return -1;
      }

      return _objectEnterCountDictionary[type];
   }

   private void UpdateKeyFunctions()
   {
      switch (CurrentType)
      {
         case ControllableObject.Type.Soul:
            References.Events.ChangeKeyFunction(0, "");
            References.Events.ChangeKeyFunction(1, "");
            References.Events.ChangeKeyFunction(2, "");
            References.Events.ChangeKeyFunction(3, "");
            References.Events.ChangeKeyFunction(4, "");
            break;
         case ControllableObject.Type.Cloud:
            References.Events.ChangeKeyFunction(0, "");
            References.Events.ChangeKeyFunction(1, "");
            References.Events.ChangeKeyFunction(2, "");
            References.Events.ChangeKeyFunction(3, References.Io.GetData().msgLeaveCloud);
            References.Events.ChangeKeyFunction(4, References.Io.GetData().msgRain);
            break;
         case ControllableObject.Type.Ground:
            References.Events.ChangeKeyFunction(0, References.Io.GetData().msgTiltLeft);
            References.Events.ChangeKeyFunction(1, References.Io.GetData().msgLeaveGround);
            References.Events.ChangeKeyFunction(2, References.Io.GetData().msgTiltRight);
            References.Events.ChangeKeyFunction(3, "");
            References.Events.ChangeKeyFunction(4, References.Io.GetData().msgShake);
            break;
         case ControllableObject.Type.Cart:
            References.Events.ChangeKeyFunction(0, "");
            References.Events.ChangeKeyFunction(1, References.Io.GetData().msgLeaveCart);
            References.Events.ChangeKeyFunction(2, "");
            References.Events.ChangeKeyFunction(3, "");
            References.Events.ChangeKeyFunction(4, References.Io.GetData().msgBrake);
            break;
         case ControllableObject.Type.Handcar:
            References.Events.ChangeKeyFunction(0, "");
            References.Events.ChangeKeyFunction(1, References.Io.GetData().msgLeaveHandcar);
            References.Events.ChangeKeyFunction(2, "");
            References.Events.ChangeKeyFunction(3, "");
            References.Events.ChangeKeyFunction(4, References.Io.GetData().msgBoost);
            break;
      }
   }
   
   public void TransferSoulTo(ControllableObject target)
   {
      if (!CanTransferSoul && target.ObjectType != ControllableObject.Type.Soul) return;

      if (_isTransferActive)
      {
         _transferSoulTimer += Time.deltaTime;
      }
      else
      {
         _beginAnimateTime = References.Timer.GetLevelTime();
         _isTransferActive = true;
      }

      while (_transferPixelCount < _transferSoulTimer * TransferPixelsPerSecond * Utility.EaseInOut(_transferSoulTimer / TransferSoulDuration) && _isVisible)
      {
         _currentTransferPixel = References.Prefabs.GetSoulPixel();
         _currentTransferPixel.SetColor(Color);
         _bezierControlPoint = _currentObject.CurrentPosition +
                               (target.CurrentPosition - _currentObject.CurrentPosition) / 2f +
                               Random.insideUnitCircle * 3f;
         _currentTransferPixel.CreateBezier(_currentObject.GetSoulTargetPoint(true, GetSoulPosition().x), _bezierControlPoint,target.GetSoulTargetPoint(true, GetSoulPosition().x));
         _currentTransferPixel.MoveOnBezier(1f);
         _currentTransferPixel.OnHasReachedTarget += target.SoulPixelImpact;
         _currentTransferPixel.SetBaseScale(2f);
         _currentTransferPixel.Fade(0f, 1f);
         _currentTransferPixel.Return(1.5f);
         _transferPixelCount++;
      }

      if (_transferSoulTimer >= TransferSoulDuration)
      {
         if (Personality != null)
         {
            Personality.BeforeLeavingRole();
         }
         _currentObject.PerformSoulTransfer(this, target);
         
         if (target.ObjectType != ControllableObject.Type.Soul)
         {
            // log: animate
            Controllers.Logs.AddLog(_beginAnimateTime, this, Log.Action.Animate, Controllers.Logs.RequestActionId(), Log.Ending.Natural, target.ObjectType.ToString());
         }
         else
         {
            // log: disembody
            Controllers.Logs.AddLog(_beginAnimateTime, this, Log.Action.Disembody, Controllers.Logs.RequestActionId(), Log.Ending.Natural, target.ObjectType.ToString());
         }
         
         References.Events.ChangePlayerState(this);
      }
   }
   
   public void CancelSoulTransfer(ControllableObject.Type targetType)
   {
      if (_transferSoulTimer > 0f && _transferSoulTimer < TransferSoulDuration)
      {
         if (targetType != ControllableObject.Type.Soul)
         {
            // log: cancel animate
            Controllers.Logs.AddLog(_beginAnimateTime, this, Log.Action.CancelAnimate, Controllers.Logs.RequestActionId(), Log.Ending.Natural, targetType.ToString());
         }
         else
         {
            // log: cancel disembody
            Controllers.Logs.AddLog(_beginAnimateTime, this, Log.Action.CancelDisembody, Controllers.Logs.RequestActionId(), Log.Ending.Natural, targetType.ToString());
         }
      }
      _transferSoulTimer = 0f;
      _transferPixelCount = 0;
      _isTransferActive = false;
   }

   public void ReturnToSoul(bool forceCompletion = false)
   {
      CanTransferSoul = true;
      if (_currentObject)
      {
         if (forceCompletion)
         {
            _currentObject.KickOut(this);
         }
         else
         {
            TransferSoulTo(_soul);  
         }
      }
   }

   public void MoveToTarget(ControllableObject target, bool kickOutPlayer = false)
   {
      if (_soul)
      {
         _soul.MoveToTarget(this, target, kickOutPlayer);
      }
   }

   public Vector2 GetCurrentPosition()
   {
      if (_currentObject == null)
      {
         return Vector2.zero;
      }

      return _currentObject.CurrentPosition;
   }

   public Vector2 GetSoulPosition()
   {
      if (_soul)
      {
         return _soul.CurrentPosition;
      }
      return Vector2.zero;
   }

   public string GetCurrentState()
   {
      if (CurrentType != ControllableObject.Type.Soul)
      {
         return CurrentType.ToString();
      }
      else
      {
         return "Pending";
      }
   }

   public void Show()
   {
      _soul.Show(1f);
      _isVisible = true;
   }

   public void Hide()
   {
      _soul.Hide(0.125f);
      _isVisible = false;
   }

   private void ThrowKickoutPixels (ControllableObject origin, ControllableObject target, int amount, float duration)
   {
      for (var i = 0; i < amount; i++)
      {
         var pixel = References.Prefabs.GetSoulPixel();
         var originPoint = origin.GetSoulTargetPoint(false, origin.CurrentPosition.x);
         var targetPoint = target.GetSoulTargetPoint(true, origin.CurrentPosition.x);
         var centerPoint = originPoint + (targetPoint - originPoint) / 2f + Random.insideUnitCircle * 12f;
         pixel.SetAlpha(0f);
         pixel.SetColor(SoulGradient.Evaluate(i / 20f));
         pixel.Fade(0.25f, duration / 2f);
         pixel.Fade(0f, duration / 4f, duration * 0.75f);
         pixel.SetPosition(originPoint);
         pixel.SetBaseScale(4f);
         pixel.CreateBezier(originPoint, centerPoint, targetPoint);
         pixel.MoveOnBezier(duration, true);
         pixel.Return(duration + 0.125f);
      }
   }

   public string GetColorString(bool useAdjectiveCase = false)
   {
      // if caption or adjective got set via personality, return those:
      if (!useAdjectiveCase)
      {
         if (!string.IsNullOrEmpty(AssessmentCaption))
         {
            return AssessmentCaption;
         }
      }
      else
      {
         if (!string.IsNullOrEmpty(AssessmentColor))
         {
            return AssessmentColor;
         }  
      }
      
      // otherwise, return a default value:
      switch (ColorId)
      {
         case 0:
            return References.Io.GetData().colorRed;
         case 1:
            return References.Io.GetData().colorOrange;
         case 2:
            return References.Io.GetData().colorYellow;
         case 3:
            return References.Io.GetData().colorGreen;
         case 4:
            return References.Io.GetData().colorBlue;
         case 5:
            return References.Io.GetData().colorPurple;
         default:
            return "???";
      }
   }

   public ControllableObject GetCurrentControllableObject()
   {
      switch (CurrentType)
      {
         case ControllableObject.Type.Soul:
            return _soul;
         case ControllableObject.Type.Cloud:
            return References.Entities.Cloud;
         case ControllableObject.Type.Handcar:
            return References.Entities.Handcar;
         case ControllableObject.Type.Ground:
            return References.Entities.Ground;
         case ControllableObject.Type.Cart:
            return References.Entities.Cart;
         default:
            return null;
      }
   }

   private void InitializeDictionaries()
   {
      _timeSpentInObjectDictionary.Clear();
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.None, 0f);
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.Soul, 0f);
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.Cloud, 0f);
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.Ground, 0f);
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.Handcar, 0f);
      _timeSpentInObjectDictionary.Add(ControllableObject.Type.Cart, 0f);
      
      _objectEnterCountDictionary.Clear();
      _objectEnterCountDictionary.Add(ControllableObject.Type.None, 0);
      _objectEnterCountDictionary.Add(ControllableObject.Type.Soul, 0);
      _objectEnterCountDictionary.Add(ControllableObject.Type.Cloud, 0);
      _objectEnterCountDictionary.Add(ControllableObject.Type.Ground, 0);
      _objectEnterCountDictionary.Add(ControllableObject.Type.Handcar, 0);
      _objectEnterCountDictionary.Add(ControllableObject.Type.Cart, 0);

      _lastUpdateTime = 0f;
   }
}
