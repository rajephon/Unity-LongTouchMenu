using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LongTouchMenuController : MonoBehaviour {
	public GameObject canvasObject = null;
	public int activeFrame = 7;

	[Header("길게 터치 중 나타날 이미지")]
	public Sprite spinnerSprite = null;
	[Header("이미지 갯수")]	
	public int spinnerSize = 12;

	[Range(0.1f, 150.0f)]
	public float spinnerRadius = 120.0f;

	public float buttonDefaultScale = 1.0f;
	public float buttonTouchUpScale = 1.2f;

	/**
	반드시 Button Component를 가진 GameObject를 넣어야 함
	 */
	public List<GameObject> menuButtonList;

	public class MenuButtonController {
		private GameObject btnGroup = null;

		public class LongTouchButton {
			public GameObject btn = null;
			public LongTouchButtonController controller = null;
		}
		private List<LongTouchButton> btnList;
		public MenuButtonController(Canvas canvas, List<GameObject> btnList, float radius, float buttonDefaultScale, float buttonTouchUpScale) {
			this.btnList = new List<LongTouchButton>();

			btnGroup = new GameObject();
			btnGroup.AddComponent<RectTransform>();
			btnGroup.transform.SetParent(canvas.transform);
			btnGroup.name = "LongTouchButtonMenu";
			float pi = Mathf.PI / 180;
			radius *= 1.5f;
			for (int i = 0; i < btnList.Count; i++) {
				if (btnList[i] == null) {
					continue;
				}
				LongTouchButton newBtn = new LongTouchButton();
				newBtn.btn = btnList[i];
				newBtn.btn.transform.SetParent(btnGroup.transform);
				float fixedAngle = 135.0f - (i * 35);
				float x = Mathf.Cos(pi * fixedAngle) * radius;
				float y = Mathf.Sin(pi * fixedAngle) * radius;
				newBtn.btn.transform.localPosition = new Vector2(x, y);

				LongTouchButtonController controller = newBtn.btn.AddComponent<LongTouchButtonController>();
				controller.defaultScaleVal = buttonDefaultScale;
				controller.increaseScaleVal = buttonTouchUpScale;
				newBtn.controller = controller;
				this.btnList.Add(newBtn);
			}
			btnGroup.SetActive(false);
		}
		public void show(Vector2 position) {
			btnGroup.transform.position = position;
			btnGroup.SetActive(true);
		}
		public void touchUp() {
			btnGroup.SetActive(false);
			// TODO: ButtonUp 이벤트 넘겨주기
			for (int i = 0; i < btnList.Count; i++) {
				btnList[i].controller.touchUp();
			}
		}
		public bool isActive() {
			return btnGroup.activeSelf;
		}
		public List<LongTouchButton> getButtonList() {
			return this.btnList;
		}
	}

	private class MenuLoadingSpinner {
		private GameObject dotGroup = null;
		public MenuLoadingSpinner(Canvas canvas) : this(canvas, null, 12, 15.0f) { }
		public MenuLoadingSpinner(Canvas canvas, Sprite spinnerSprite, int spinnerSize) : this(canvas, spinnerSprite, spinnerSize, 15.0f) {}
		public MenuLoadingSpinner(Canvas canvas, Sprite spinnerSprite, int spinnerSize, float radius) {
			if (canvas != null) {
				dotGroup = new GameObject();
				dotGroup.transform.SetParent(canvas.transform);
				dotGroup.SetActive(false);
				dotGroup.name = "LoadingSpinner";
				for (int i = 0; i < spinnerSize; i++) {
					GameObject dot = new GameObject();
					Image dotImage = dot.AddComponent<Image>();
					dot.transform.SetParent(dotGroup.transform);
					RectTransform rectTransform = dot.GetComponent<RectTransform>();
					
					 if (spinnerSprite != null) {
						dotImage.sprite = spinnerSprite;
						Vector3 origSize = spinnerSprite.bounds.size;
						rectTransform.sizeDelta = new Vector2(origSize.x, origSize.y);
					 }else {
						rectTransform.sizeDelta = new Vector2(10, 10);
					 }
				}
				positioning(radius);
			}
		}
		private void positioning(float radius) {
			int childCount = dotGroup.transform.childCount;
			float pi = Mathf.PI / 180;
			float baseAngle = 360.0f / childCount;
			for (int i = 0; i < childCount; i++) {
				Transform dotRectTransform = dotGroup.transform.GetChild(i);
				// 마우스 보다 위쪽에서 시계방향의 회전을 위해 각도 수치 보정
				float x = Mathf.Cos(pi * (baseAngle * i + 90)) * radius * -1;
				float y = Mathf.Sin(pi * (baseAngle * i + 90)) * radius;
				dotRectTransform.localPosition = new Vector2(x, y);
			}
		}

		/**
		value는 0.0f ~ 1.0f 사이값으로 들어와야 한다.
		 */
		public void progress(float psvalue) {
			int childCount = dotGroup.transform.childCount;
			for (int i = 0; i < childCount; i++) {
				dotGroup.transform.GetChild(i).gameObject.SetActive(psvalue > ((float)i / (float)childCount));
			}
		}
		public void show(Vector2 position, float radius) {
			if (dotGroup != null && !dotGroup.activeSelf) {
				dotGroup.transform.position = new Vector2(position.x, position.y);
			}
			positioning(radius);
			dotGroup.SetActive(true);
		}
		public void hide() {
			dotGroup.SetActive(false);
		}
		public bool isActive() {
			return dotGroup.activeSelf;
		}
	}

	private int durationCount = 0;
	private MenuLoadingSpinner menuLoadingSpinner = null;
	private MenuButtonController menuBtnController = null;
	// Use this for initialization
	void Start () {
		// canvasObject = GameObject.Find("Canvas");
		if (canvasObject != null) {
			if (canvasObject.GetComponent<Canvas>()!=null) {
            	Canvas canvas=canvasObject.GetComponent<Canvas>();
				menuLoadingSpinner = new MenuLoadingSpinner(canvas, spinnerSprite, spinnerSize);
				menuBtnController = new MenuButtonController(canvas, menuButtonList, spinnerRadius, buttonDefaultScale, buttonTouchUpScale);
			}else{
				Debug.Log("Can't find canvas component");
			}
		}else {
			Debug.Log("Can't find canvas object");
		}
	}
	
	// Update is called once per frame
	private bool preMouseBtnStatus = false;
	static bool isTouchBlocked = false;
	private Vector2 getTouchPosition() {
		Vector2 position = new Vector2(0, 0);
		if (Input.GetMouseButton(0)) {
			position.x = Input.mousePosition.x;
			position.y = Input.mousePosition.y;
		}else if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) {
			position.x = Input.GetTouch(0).position.x;
			position.y = Input.GetTouch(0).position.y;
		}
		return position;
	}
	
	/**
	 * Canvas UI Object 위에서의 터치인지 확인.
	 * 터치->드래그로 움직이더라도, LongTouch Menu 등장을 막기 위해서 bool isTouchBlocked 활용.
	 */
	private bool touchOnCanvasUIObject() {
		if (!isTouchBlocked) {
			isTouchBlocked = EventSystem.current.IsPointerOverGameObject() || 
							( Input.touchCount == 1  && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId));
		}
		return isTouchBlocked;
	}

	void Update () {
		if (menuBtnController != null) {
			if ( ! menuBtnController.isActive()) { // 메뉴 버튼이 나오지 않은 상태일 때
				if (((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetMouseButton(0)) && !touchOnCanvasUIObject() ) {
					if (!menuLoadingSpinner.isActive()) {
						Vector2 touchPosition = getTouchPosition();
						menuLoadingSpinner.show(touchPosition, spinnerRadius);
					}
					durationCount++;
					if (durationCount > activeFrame) {
						Vector2 touchPosition = getTouchPosition();
						menuBtnController.show(touchPosition);
						menuLoadingSpinner.hide();
						durationCount = 0;
					}else {
						menuLoadingSpinner.progress((float)durationCount / (float)activeFrame);
					}
				}else if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) || (preMouseBtnStatus && !Input.GetMouseButton(0))) {
					// 터치 끝
					durationCount = 0;
					menuLoadingSpinner.hide();
					isTouchBlocked = false;
				}
			}else { // 메뉴 버튼이 나온 상태일 때.
				// Vector2 touchPosition = getTouchPosition();
				// if ((Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(0).phase == TouchPhase.Moved)) || Input.GetMouseButton(0)) {
					// if (!preMouseBtnStatus && !touchOnCanvasUIObject()) {
						// menuBtnController.hide();
					// }
					// TODO: 메뉴 버튼 위에 올라갔는지 확인.
				// }else
				if ((Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended) || (preMouseBtnStatus && !Input.GetMouseButton(0))) {
					// TODO: 메뉴 버튼 위에서 사라졌으면 해당 버튼 onClick 발동 해주고 사라지기.
					// CursorRay = Camera.main.ScreenPointToRay(Input.GetTouch(0).position );
					isTouchBlocked = false;
					menuBtnController.touchUp();
				}
			}
		}else {
			Debug.Log("menuBtnController is null!!");
		}
		
		preMouseBtnStatus = Input.GetMouseButton(0);

	}
}
