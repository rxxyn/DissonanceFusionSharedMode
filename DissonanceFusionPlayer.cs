using System.Collections;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.PhotonFusion
{
    public class DissonanceFusionPlayer
        : NetworkBehaviour, IDissonancePlayer
    {
        #region fields & properties
        private static readonly Log Log = Logs.Create(LogCategory.Core, nameof(DissonanceFusionPlayer));

        [CanBeNull] private Transform _transform;
        private Transform Transform
        {
            get
            {
                if (_transform == null)
                    _transform = transform;
                return _transform;
            }
        }

        public Vector3 Position => Transform.position;
        public Quaternion Rotation => Transform.rotation;

        public NetworkPlayerType Type
        {
            get
            {
                if (Runner == null)
                    return NetworkPlayerType.Unknown;

                if (Runner.IsClient || Runner.IsServer)
                {
                    if (HasInputAuthority)
                        return NetworkPlayerType.Local;
                    else
                        return NetworkPlayerType.Remote;
                }

                return NetworkPlayerType.Unknown;
            }
        }

        // This can only be set by the server
        [Networked(OnChanged = nameof(OnNetworkedPlayerNameChangedStatic), OnChangedTargets = OnChangedTargets.All)]
        [Capacity(64)]
        private string NetworkedPlayerName { get; set; }

        public bool IsTracking { get; private set; }
        public string PlayerId { get; private set; }

        [CanBeNull] private DissonanceComms _dissonance;
        #endregion

        #region Fusion spawn/despan
        public override void Spawned()
        {
            base.Spawned();

            Log.Debug($"Spawned HasInputAuthority={HasInputAuthority}, HasStateAuthority={HasStateAuthority}");

            StartCoroutine(OnSpawnedCo());
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);

            StopAllCoroutines();

            if (IsTracking && _dissonance != null)
            {
                _dissonance.StopTracking(this);
                IsTracking = false;
            }

            if (_dissonance != null)
                _dissonance.LocalPlayerNameChanged -= SetLocalPlayerName;
        }

        private IEnumerator OnSpawnedCo()
        {
            // Wait until Dissonance comms object is created
            if (_dissonance == null)
            {
                while (_dissonance == null)
                {
                    _dissonance = FindObjectOfType<DissonanceComms>();
                    yield return null;
                }
            }

            // Set initial name if this is the local player
            if (HasInputAuthority)
            {
                _dissonance.LocalPlayerNameChanged += SetLocalPlayerName;
                SetLocalPlayerName(_dissonance.LocalPlayerName);
            }
        }
        #endregion

        #region name changes
        protected static void OnNetworkedPlayerNameChangedStatic(Changed<DissonanceFusionPlayer> changed)
        {
            changed.Behaviour.OnNetworkedPlayerNameChanged(changed);
        }

        protected void OnNetworkedPlayerNameChanged(Changed<DissonanceFusionPlayer> changed)
        {
            Log.Debug($"OnNetworkedPlayerNameChanged HasInputAuthority={HasInputAuthority}, HasStateAuthority={HasStateAuthority}");

            StopAllCoroutines();
            if (IsTracking)
                StopTracking();

            PlayerId = NetworkedPlayerName;
            StartTracking();
        }

        private void SetLocalPlayerName(string dissonanceName)
        {
            Rpc_SetName(dissonanceName);
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void Rpc_SetName(string name)
        {
            NetworkedPlayerName = name;
        }
        #endregion

        #region start/stop tracking
        private void StartTracking()
        {
            if (IsTracking)
                throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "0663D808-ACCC-4D13-8913-03F9BA0C8578");

            StopAllCoroutines();
            StartCoroutine(StartTrackingCo());
        }

        private IEnumerator StartTrackingCo()
        {
            // Wait until Dissonance comms object is created
            if (_dissonance == null)
            {
                while (_dissonance == null)
                {
                    _dissonance = FindObjectOfType<DissonanceComms>();
                    yield return null;
                }
            }

            // Can't track someone with a null name! Loop until name is valid
            while (PlayerId == null)
                yield return null;

            // Now start tracking
            Debug.LogWarning("Tracking");
            _dissonance.TrackPlayerPosition(this);
            IsTracking = true;
        }

        private void StopTracking()
        {
            if (!IsTracking)
                throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "48802E32-C840-4C4B-BC58-4DC741464B9A");

            // Stop startup coroutine if it is running
            StopAllCoroutines();
            Debug.LogWarning("Stopped Tracking");

            // No need to search for Dissonance, if we don't already have a reference then we can't already be tracking
            if (_dissonance != null)
            {
                _dissonance.StopTracking(this);
                IsTracking = false;
            }
        }
        #endregion
    }
}
