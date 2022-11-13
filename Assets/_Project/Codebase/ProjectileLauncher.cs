using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace _Project.Codebase
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField] private GameObject _projectileFab;
        public float fireDelay;
        public float launchSpeed;

        private float _lastFireTime;
        
        public void Fire()
        {
            if (Time.time < _lastFireTime + fireDelay)
                return;
            
            _lastFireTime = Time.time;
            Projectile projectile = Instantiate(_projectileFab).GetComponent<Projectile>();
            projectile.transform.right = transform.right;
            projectile.speed = launchSpeed;
        }
    }
}