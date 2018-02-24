using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LongTouchButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public float defaultScaleVal = 1.0f;
	public float increaseScaleVal = 1.2f;
	private bool isPointerEnter = false;

	// // Use this for initialization
	// void Start () {
		
	// }
	// // Update is called once per frame
	// void Update () {
		
	// }

	public void OnPointerExit(PointerEventData eventData) {
		isPointerEnter = false;
		setDefaultScale();
    }

	public void OnPointerEnter(PointerEventData eventData) {
		isPointerEnter = true;
		setIncreaseScale();
	}

	private void setIncreaseScale() {
		this.transform.localScale = new Vector3(increaseScaleVal, increaseScaleVal, increaseScaleVal);
	}

	private void setDefaultScale() {
		this.transform.localScale = new Vector3(defaultScaleVal, defaultScaleVal, defaultScaleVal);
	}

	/**
	 * 버튼 hide처리, scaleUp 상태라면 Scale 다시 낮추고 click 이벤트 동작.
	 */
	public void touchUp() {
		if (isPointerEnter) {
			isPointerEnter = false;
			setDefaultScale();
			Button btn = this.GetComponent<Button>();
			btn.onClick.Invoke();
		}
	}

}
