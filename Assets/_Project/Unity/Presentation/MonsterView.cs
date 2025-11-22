using UnityEngine;

namespace SubGame.Unity.Presentation
{
    /// <summary>
    /// Visual representation of a monster entity.
    /// </summary>
    public class MonsterView : EntityView
    {
        [Header("Monster Visuals")]
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Color _normalColor = Color.red;
        [SerializeField] private Color _activeColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private Color _damagedColor = Color.white;

        [Header("Health Bar")]
        [SerializeField] private Transform _healthBarFill;
        [SerializeField] private GameObject _healthBarRoot;

        private Material _material;
        private float _damageFlashTimer;
        private const float DamageFlashDuration = 0.3f;
        private bool _isDead;

        private void Awake()
        {
            if (_renderer != null)
            {
                // Create instance of material to avoid affecting other monsters
                _material = _renderer.material;
                _material.color = _normalColor;
            }
        }

        private void OnDestroy()
        {
            // Clean up instanced material
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        /// <inheritdoc/>
        public override void UpdateHealth(int currentHealth, int maxHealth)
        {
            if (_healthBarFill != null && maxHealth > 0)
            {
                float healthPercent = (float)currentHealth / maxHealth;
                _healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
            }

            if (_healthBarRoot != null)
            {
                _healthBarRoot.SetActive(currentHealth < maxHealth);
            }
        }

        /// <inheritdoc/>
        public override void OnDamageTaken(int damage)
        {
            _damageFlashTimer = DamageFlashDuration;
        }

        /// <inheritdoc/>
        public override void OnDeath()
        {
            _isDead = true;

            // Simple death effect - could be expanded with particles, animation, etc.
            if (_material != null)
            {
                _material.color = Color.gray;
            }

            // Disable health bar
            if (_healthBarRoot != null)
            {
                _healthBarRoot.SetActive(false);
            }
        }

        /// <inheritdoc/>
        public override void SetSelected(bool selected)
        {
            if (_isDead || _material == null || _damageFlashTimer > 0)
                return;

            _material.color = selected ? _activeColor : _normalColor;
        }

        /// <inheritdoc/>
        protected override void Update()
        {
            base.Update();

            // Don't update visuals if dead
            if (_isDead)
                return;

            // Handle damage flash
            if (_damageFlashTimer > 0)
            {
                _damageFlashTimer -= Time.deltaTime;

                if (_material != null)
                {
                    float t = _damageFlashTimer / DamageFlashDuration;
                    _material.color = Color.Lerp(_normalColor, _damagedColor, t);
                }
            }
        }
    }
}
