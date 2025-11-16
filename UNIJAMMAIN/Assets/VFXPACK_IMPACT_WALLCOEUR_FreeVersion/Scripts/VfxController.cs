namespace Assets.VFXPACK_IMPACT_WALLCOEUR.Scripts
{
    using System.Collections.Generic;
    using UnityEngine;

    public class VfxController : MonoBehaviour
    {
        [Header("Place VFX prefabs here")]
        [SerializeField] List<GameObject> _vfxList;
        public List<GameObject> VfxList { get => _vfxList; }

        GameObject _currentVfx;

        private void Start()
        {
            Managers.Game.vfxController = this;
        }

        public GameObject GetVfx(int index,Vector2 position)
        {
            _currentVfx = Instantiate(_vfxList[index], position, Quaternion.identity, transform);
            return _currentVfx;
           
        }

        public void Stop()
        {
            if (_currentVfx != null)
            {
                Destroy(_currentVfx);
            }
        }
    }
}