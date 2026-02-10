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
            if (pistol.gameObject.activeInHierarchy)
                pistol.FireInput(virtualFireState);
            if (shotgun.gameObject.activeInHierarchy)
                shotgun.FireInput(virtualFireState);
            if (m4.gameObject.activeInHierarchy)
                m4.FireInput(virtualFireState);
            if (m79Grenadelauncher.gameObject.activeInHierarchy)
                m79Grenadelauncher.FireInput(virtualFireState);
            if (beamGun.gameObject.activeInHierarchy)
                beamGun.FireInput(virtualFireState);
            if (railGun.gameObject.activeInHierarchy)
                railGun.FireInput(virtualFireState);
            if (rocketLauncher.gameObject.activeInHierarchy)
                rocketLauncher.FireInput(virtualFireState);
            if (clusterBombLauncher.gameObject.activeInHierarchy)
                clusterBombLauncher.FireInput(virtualFireState);
        }

        public void VirtualSwitchInput(bool virtualSwitchState)
        {
            weaponSystem.SwitchInput(virtualSwitchState);
        }

        public void VirtualReloadInput(bool virtualReloadState)
        {
            if (pistol.gameObject.activeInHierarchy)
                pistol.ReloadInput(virtualReloadState);
            if (shotgun.gameObject.activeInHierarchy)
                shotgun.ReloadInput(virtualReloadState);
            if (m4.gameObject.activeInHierarchy)
                m4.ReloadInput(virtualReloadState);
            if (m79Grenadelauncher.gameObject.activeInHierarchy)
                m79Grenadelauncher.ReloadInput(virtualReloadState);
            if (beamGun.gameObject.activeInHierarchy)
                beamGun.ReloadInput(virtualReloadState);
            if (railGun.gameObject.activeInHierarchy)
                railGun.ReloadInput(virtualReloadState);
            if (rocketLauncher.gameObject.activeInHierarchy)
                rocketLauncher.ReloadInput(virtualReloadState);
            if (clusterBombLauncher.gameObject.activeInHierarchy)
                clusterBombLauncher.ReloadInput(virtualReloadState);
        }

    }

}
