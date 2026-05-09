SYSTEMS ENGINEER PREFAB SETUP CHECKLIST

After dragging prefabs into a new scene, relink these references:

1. Main Camera
- ThirdPersonCamera → Target = player/CameraTarget

2. Player
- ThirdPersonPlayerController → Camera Transform = Main Camera
- PlayerShooting → Player Camera = Main Camera
- PlayerShooting → Ammo Text = GameplayCanvas/AmmoText
- PlayerShooting → Interaction UI = GameplayCanvas
- PlayerInteraction → Interaction UI = GameplayCanvas
- PlayerObjectInteraction → Carry Point = player/CarryPoint
- PlayerObjectInteraction → Interaction UI = GameplayCanvas
- PlayerHealth → Health Text = GameplayCanvas/HealthText
- PlayerHealth → Interaction UI = GameplayCanvas

3. GameplayCanvas
- InteractionUI → Interaction Text = InteractionText
- InteractionUI → Feedback Text = FeedbackText

4. Scene requirements
- Scene must have a ground/floor collider
- Only one active Main Camera
- Only one active EventSystem
- Player must not spawn inside walls or props