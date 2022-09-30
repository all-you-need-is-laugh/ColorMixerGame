using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Responds for camera movement

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Transform _resultsViewPoint;

    [SerializeField]
    private float _animationDuration = 1;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    public new Camera camera { get; private set; }
    private Vector3 _startPosition;
    private Vector3 _startRotation;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    void Start() {
        camera = GetComponent<Camera>();

        _startPosition = transform.position;
        _startRotation = transform.rotation.eulerAngles;
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task MoveToResultsViewAsync() {
        return DOTween.Sequence()
            .Join(transform.DOMove(_resultsViewPoint.position, _animationDuration))
            .Join(transform.DORotate(_resultsViewPoint.rotation.eulerAngles, _animationDuration))
            .AsyncWaitForCompletion();
    }

    public Task MoveToStartPointAsync() {
        return DOTween.Sequence()
            .Join(transform.DOMove(_startPosition, _animationDuration))
            .Join(transform.DORotate(_startRotation, _animationDuration))
            .AsyncWaitForCompletion();
    }

    #endregion Main functionality -------------------------------------------------
}
