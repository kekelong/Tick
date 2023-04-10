using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;

namespace InfimaGames.Animated.ModernGuns
{
    public class Bullet: MonoBehaviour
    {
        #region FIELDS SERIALIZED


        [Header("Settings")]

        [Tooltip("bullet speed")]
        public float speed = 500;

        [Tooltip("time to live")]
        public float liveTime = 15f;
     
        public AudioClip[] bulletSounds;

        #endregion

        #region FIELDS
        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// Audio Source Component.
        /// </summary>
        public AudioSource audioSource;
        /// <summary>
        /// Instantiated Particle System.
        /// </summary>
        public GameObject particlesGO;
        private ParticleSystem particles;

        // Cast the ray and handle the hit
        RaycastHit hit;
        Vector3 hitPoint;
        Ray ray;
        #endregion

        #region UNITY


        private void Start()
        {
            //We need to get the character component.
            playerCharacter ??= ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
            //Check Reference.
            if (playerCharacter == null)
            {
                Debug.Log("character controller is null");
                return;
            }

            CharacterController controller = playerCharacter.transform.parent.GetComponent<CharacterController>();
            Physics.IgnoreCollision(controller, GetComponent<Collider>());

            //射线检测，确定子弹射出方向路径
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.Log("main camera  is null");
                return;
            }
            ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out hit))
            {
                hitPoint = hit.point;
                Debug.Log("hit point = " + hit.point +"--"+ hit.collider.name);
            }
            else
            {
                hitPoint = ray.origin+ ray.direction.normalized * 500; ;
            }

            if (particlesGO != null)
            {
                GameObject particlesInstance = Instantiate(particlesGO, hitPoint, Quaternion.identity);
                particles = particlesInstance.GetComponent<ParticleSystem>();
            }
        }

        private void Update()
        {
            // 将方向向量转换为旋转
            Quaternion rotation = Quaternion.LookRotation(ray.direction.normalized, Vector3.up);
            // 应用旋转
            transform.rotation = rotation;
            // 沿着z轴移动
            transform.Translate(Vector3.forward * 200 * Time.deltaTime);



            //transform.Translate(transform.TransformDirection(shootDirection) * 5 *Time.deltaTime);
            //Debug.Log(transform.position);
            //// 计算物体需要移动的向量
            //Vector3 moveVector = endPoint - objectTransform.position;
            //// 将物体平滑地移动到目标点
            //objectTransform.position = Vector3.MoveTowards(objectTransform.position, endPoint, speed * Time.deltaTime);

            //// 计算物体需要旋转的角度，使得自身的z轴指向目标点
            //Quaternion targetRotation = Quaternion.LookRotation(moveVector, -objectTransform.up);

            //// 将物体的旋转角度平滑地调整到目标角度
            //objectTransform.rotation = Quaternion.RotateTowards(objectTransform.rotation, targetRotation, 2 * Time.deltaTime);
            ////transform.position += new Vector3(0,0,speed*Time.deltaTime);
        }

        #endregion

        #region FUNCTIONS


        private void OnCollisionEnter(Collision collision)
        {
            if (particles != null)
            {
                particles.Play();  
            }
            else
            {
                Debug.Log(" particles  null");
            }
            Destroy(gameObject);
        }

        #endregion
    }
}
