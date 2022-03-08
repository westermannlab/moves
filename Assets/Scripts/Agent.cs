using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : ControllableObject
{
   protected override void Awake()
   {
      base.Awake();
      CurrentPlayer.CurrentType = ObjectType;
   }

   private void FixedUpdate()
   {
      CurrentPosition = Tf.position;
   }
   
   public override void ProcessInput(InputController.Type inputType, InputController.Mode inputMode, float x = 0f, float y = 0f)
   {
      base.ProcessInput(inputType, inputMode);
      switch (inputType)
      {
         case InputController.Type.Left:
            // dirty hack
            if (References.Entities.Handcar.transform.position.x < CurrentPosition.x 
                && ((Vector2) References.Entities.Handcar.transform.position - CurrentPosition).sqrMagnitude < 2.25f)
            {
               CurrentPlayer.TransferSoulTo(References.Entities.Handcar);
               References.Entities.PlayerTwo.TransferSoulTo(References.Entities.Ground);
               break;
            }
            MoveLeft();
            break;
         case InputController.Type.Right:
            // dirty hack
            if (References.Entities.Cart.transform.position.x > CurrentPosition.x 
                && ((Vector2) References.Entities.Cart.transform.position - CurrentPosition).sqrMagnitude < 2.25f)
            {
               CurrentPlayer.TransferSoulTo(References.Entities.Cart);
               References.Entities.PlayerTwo.TransferSoulTo(References.Entities.Ground);
               break;
            }
            MoveRight();
            break;
         case InputController.Type.Up:
            if (inputMode == InputController.Mode.Hold)
            {
               CurrentPlayer.TransferSoulTo(References.Entities.Cloud);
            }
            else if (inputMode == InputController.Mode.Release)
            {
               CurrentPlayer.CancelSoulTransfer(Type.Cloud);
            }
            break;
         case InputController.Type.Down:
            if (inputMode == InputController.Mode.Hold)
            {
               CurrentPlayer.TransferSoulTo(References.Entities.Ground);
            }
            else if (inputMode == InputController.Mode.Release)
            {
               CurrentPlayer.CancelSoulTransfer(Type.Ground);
            }
            break;
         case InputController.Type.Space:
            if (inputMode == InputController.Mode.Press)
            {
               Jump();
            }
            break;
      }
   }

   public override void Enter(Player player)
   {
      base.Enter(player);
      ChangeAlpha(1f, 1f);
      Rb.simulated = true;
   }

   public override void Leave(Player player)
   {
      base.Leave(player);
      ChangeAlpha(0f, 1f);
      Rb.simulated = false;
   }

   public override Vector2 GetSoulTargetPoint(bool addRandom, float xOffset = 0f)
   {
      return CurrentPosition + (addRandom ? Random.insideUnitCircle * 0.375f : Vector2.zero);
   }

   private void Jump()
   {
      AddForce(Vector2.up, 250f);
   }
}
