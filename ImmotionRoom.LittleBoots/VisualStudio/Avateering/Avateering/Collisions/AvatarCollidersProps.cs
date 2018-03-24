namespace ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    /// <summary>
    /// Provides constants and helper methods for colliders of avatars
    /// </summary>
    public class AvatarCollidersProps
    {
        #region Constants definitions

        /// <summary>
        /// Constants about Left hand colliders
        /// </summary>
        public static readonly AvatarColliderData LeftHandColliders = new AvatarColliderData() { ObjectName = "#ir#_H_L", ObjectTag = "#ir#_H_L" };

        /// <summary>
        /// Constants about Right hand colliders
        /// </summary>
        public static readonly AvatarColliderData RightHandColliders = new AvatarColliderData() { ObjectName = "#ir#_H_R", ObjectTag = "#ir#_H_R" };

        /// <summary>
        /// Constants about Left foot colliders
        /// </summary>
        public static readonly AvatarColliderData LeftFootColliders = new AvatarColliderData() { ObjectName = "#ir#_F_L", ObjectTag = "#ir#_F_L" };

        /// <summary>
        /// Constants about Right foot colliders
        /// </summary>
        public static readonly AvatarColliderData RightFootColliders = new AvatarColliderData() { ObjectName = "#ir#_F_R", ObjectTag = "#ir#_F_R" };

        #endregion

        #region Helper methods

        /// <summary>
        /// Gets if the provided collider represents a left hand of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar hands' one, false otherwise</returns>
        public static bool IsAvatarLeftHandCollider(Collider collider)
        {
            return (collider.gameObject.name == LeftHandColliders.ObjectName);
        }

        /// <summary>
        /// Gets if the provided collider represents a right hand of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar hands' one, false otherwise</returns>
        public static bool IsAvatarRightHandCollider(Collider collider)
        {
            return (collider.gameObject.name == RightHandColliders.ObjectName);
        }

        /// <summary>
        /// Gets if the provided collider represents a hand of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar hands' one, false otherwise</returns>
        public static bool IsAvatarHandCollider(Collider collider)
        {
            return IsAvatarLeftHandCollider(collider) || IsAvatarRightHandCollider(collider);
        }

        /// <summary>
        /// Gets if the provided collider represents a left foot of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar foots' one, false otherwise</returns>
        public static bool IsAvatarLeftFootCollider(Collider collider)
        {
            return (collider.gameObject.name == LeftFootColliders.ObjectName);
        }

        /// <summary>
        /// Gets if the provided collider represents a right foot of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar foots' one, false otherwise</returns>
        public static bool IsAvatarRightFootCollider(Collider collider)
        {
            return (collider.gameObject.name == RightFootColliders.ObjectName);
        }

        /// <summary>
        /// Gets if the provided collider represents a foot of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar foots' one, false otherwise</returns>
        public static bool IsAvatarFootCollider(Collider collider)
        {
            return IsAvatarLeftFootCollider(collider) || IsAvatarRightFootCollider(collider);
        }

        /// <summary>
        /// Gets if the provided collider represents a hand or foot collider of a ImmotionRoom avatar.
        /// The method uses collider object names to determine the results
        /// </summary>
        /// <param name="collider">Collider to check</param>
        /// <returns>True if collider is a avatar hands' one, false otherwise</returns>
        public static bool IsAvatarCollider(Collider collider)
        {
            return IsAvatarFootCollider(collider) || IsAvatarHandCollider(collider);
        }

        /// <summary>
        /// Obtain the left hand collider relative to the provided avatar
        /// </summary>
        /// <param name="avatarRootTransform">Root transform of an avatar.</param>
        /// <returns>Left hand inside the transform tree of the provided element, or null if no left hand is present</returns>
        public static Collider GetAvatarLeftHandCollider(Transform avatarRootTransform)
        {
            //get all colliders inside the transform tree
            Collider[] colliders = avatarRootTransform.GetComponentsInChildren<Collider>();

            //loop all colliders and return the first left hand one, if any
            foreach (Collider collider in colliders)
                if (IsAvatarLeftHandCollider(collider))
                    return collider;

            //if we are here, we found nothing. Return null
            return null;
        }

        /// <summary>
        /// Obtain the right hand collider relative to the provided avatar
        /// </summary>
        /// <param name="avatarRootTransform">Root transform of an avatar.</param>
        /// <returns>Right hand inside the transform tree of the provided element, or null if no right hand is present</returns>
        public static Collider GetAvatarRightHandCollider(Transform avatarRootTransform)
        {
            //get all colliders inside the transform tree
            Collider[] colliders = avatarRootTransform.GetComponentsInChildren<Collider>();

            //loop all colliders and return the first right hand one, if any
            foreach (Collider collider in colliders)
                if (IsAvatarRightHandCollider(collider))
                    return collider;

            //if we are here, we found nothing. Return null
            return null;
        }

        /// <summary>
        /// Obtain the left foot collider relative to the provided avatar
        /// </summary>
        /// <param name="avatarRootTransform">Root transform of an avatar.</param>
        /// <returns>Left foot inside the transform tree of the provided element, or null if no left foot is present</returns>
        public static Collider GetAvatarLeftFootCollider(Transform avatarRootTransform)
        {
            //get all colliders inside the transform tree
            Collider[] colliders = avatarRootTransform.GetComponentsInChildren<Collider>();

            //loop all colliders and return the first left foot one, if any
            foreach (Collider collider in colliders)
                if (IsAvatarLeftFootCollider(collider))
                    return collider;

            //if we are here, we found nothing. Return null
            return null;
        }

        /// <summary>
        /// Obtain the right foot collider relative to the provided avatar
        /// </summary>
        /// <param name="avatarRootTransform">Root transform of an avatar.</param>
        /// <returns>Right foot inside the transform tree of the provided element, or null if no right foot is present</returns>
        public static Collider GetAvatarRightFootCollider(Transform avatarRootTransform)
        {
            //get all colliders inside the transform tree
            Collider[] colliders = avatarRootTransform.GetComponentsInChildren<Collider>();

            //loop all colliders and return the first right foot one, if any
            foreach (Collider collider in colliders)
                if (IsAvatarRightFootCollider(collider))
                    return collider;

            //if we are here, we found nothing. Return null
            return null;
        }

        #endregion
    }
}
