﻿using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

//namespace Komodo.Runtime
//{
    /// <summary>
    /// Used to store Entity components that maek up avatars to help ease access to the GameObjects that should receive/send network updates. 
    /// </summary>
    public class AvatarEntityGroup : MonoBehaviour
    {
        public int clientID;
        public Entity rootEntity;
        public Entity entityHead;
        public Entity entityHand_L;
        public Entity entityHand_R;

        public AvatarComponent avatarComponent_Head;
        public AvatarComponent avatarComponent_hand_L;
        public AvatarComponent avatarComponent_hand_R;

    public AvatarComponent avatarComponentHand_L;
    public AvatarComponent avatarComponentHand_R;

    public TMP_Text clientNameTag;
        public TMP_Text clientDialogue;
    }
//}