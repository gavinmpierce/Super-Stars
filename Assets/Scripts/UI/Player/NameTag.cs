using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class NameTag : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    private readonly NetworkVariable<FixedString32Bytes> playerName = new(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;
        nameText.text = playerName.Value.ToString();

        if (IsOwner)
            playerName.Value = PlayerPrefs.GetString("PlayerName", $"Player {OwnerClientId}");
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        nameText.text = newName.ToString();
    }
}