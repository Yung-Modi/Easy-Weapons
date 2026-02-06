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
            if (pistol != null)
                pistol.FireInput(virtualFireState);
            if (shotgun != null)
                shotgun.FireInput(virtualFireState);
            if (m4 != null)
                m4.FireInput(virtualFireState);
            if (m79Grenadelauncher != null)
                m79Grenadelauncher.FireInput(virtualFireState);
            if (beamGun != null)
                beamGun.FireInput(virtualFireState);
            if (railGun != null)
                railGun.FireInput(virtualFireState);
            if (rocketLauncher != null)
                rocketLauncher.FireInput(virtualFireState);
            if (clusterBombLauncher != null)
                clusterBombLauncher.FireInput(virtualFireState);
        }

        public void VirtualSwitchInput(bool virtualSwitchState)
        {
            weaponSystem.SwitchInput(virtualSwitchState);
        }

        public void VirtualReloadInput(bool virtualReloadState)
        {
            if (pistol != null)
                pistol.ReloadInput(virtualReloadState);
            if (shotgun != null)
                shotgun.ReloadInput(virtualReloadState);
            if (m4 != null)
                m4.ReloadInput(virtualReloadState);
            if (m79Grenadelauncher != null)
                m79Grenadelauncher.ReloadInput(virtualReloadState);
            if (beamGun != null)
                beamGun.ReloadInput(virtualReloadState);
            if (railGun != null)
                railGun.ReloadInput(virtualReloadState);
            if (rocketLauncher != null)
                rocketLauncher.ReloadInput(virtualReloadState);
            if (clusterBombLauncher != null)
                clusterBombLauncher.ReloadInput(virtualReloadState);
        }

    }

}
