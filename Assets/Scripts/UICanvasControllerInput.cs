using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public SlowMotion slowMotion;
        public FirstPersonCharacter firstPersonCharacter;
        public Weapon pistol, m4, shotgun, m79Grenadelauncher, beamGun, railGun, rocketLauncher, clusterBombLauncher;
        public WeaponSystem weaponSystem;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
           firstPersonCharacter.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            firstPersonCharacter.LookInput(virtualLookDirection);
        }

        public void VirtualSlowMotionInput(bool virtualSlowMotionState)
        {
            slowMotion.SlowMotionInput(virtualSlowMotionState);
        }

        public void VirtualFireInput(bool virtualFireState)
        {
            pistol.FireInput(virtualFireState);
            shotgun.FireInput(virtualFireState);
            m4.FireInput(virtualFireState);
            m79Grenadelauncher.FireInput(virtualFireState);
            beamGun.FireInput(virtualFireState);
            railGun.FireInput(virtualFireState);
            rocketLauncher.FireInput(virtualFireState);
            clusterBombLauncher.FireInput(virtualFireState);
        }

        public void VirtualSwitchInput(bool virtualSwitchState)
        {
            weaponSystem.SwitchInput(virtualSwitchState);
        }

        public void VirtualReloadInput(bool virtualReloadState)
        {
            pistol.ReloadInput(virtualReloadState);
            shotgun.ReloadInput(virtualReloadState);
            m4.ReloadInput(virtualReloadState);
            m79Grenadelauncher.ReloadInput(virtualReloadState);
            beamGun.ReloadInput(virtualReloadState);
            railGun.ReloadInput(virtualReloadState);
            rocketLauncher.ReloadInput(virtualReloadState);
            clusterBombLauncher.ReloadInput(virtualReloadState);
        }

    }

}
