/// <summary>
/// Pie Menu Z
/// Created by: Zalán Kórósi
/// Version: 2.0
/// </summary>

using UnityEngine;
using System.Collections;

[System.Serializable]
public class PieMenu {
	
	public enum Shape {
		Circle,
		Semi_Circle
	}

	public enum InputType {
		Axis,
		Mouse,
		Touch
	}

	[System.Serializable]
	public class AxisOptions {
		[Tooltip("Axis used to control the Pie Menu.")]
		public string XAxis = "Mouse X";
		[Tooltip("Axis used to control the Pie Menu.")]
		public string YAxis = "Mouse Y";

		[Range(0,2), Tooltip("Sensitivity of the axis.")]
		public float Sensitivity = 1f;
		[Range(0,2), Tooltip("Minimum distance from axis origo to respond.")]
		public float Treshold = 0.05f;
		[Tooltip("Hide the mouse cursor if the Pie Menu is opened.")]
		public bool HideCursor = false;
	}

	[System.Serializable]
	public class MouseOptions {
		[Tooltip("Minimum distance from the center to receive input.")]
		public float innerInputRadius = 10f;
		[Tooltip("Maximum distance from the center to receive input.")]
		public float outerInputRadius = 100f;
		[Tooltip("If enabled, the distance between the mouse and the pie option will be compared with OptionsStandardSize, to ensure that your cursor in on the option.")]
		public bool checkIfCursorIsOnOption = false;
	}

	[System.Serializable]
	public class TouchOptions {
		[Tooltip("Minimum distance from the center to receive input.")]
		public float innerInputRadius = 1f;
		[Tooltip("Maximum distance from the center to receive input.")]
		public float outerInputRadius = 5f;
		[HideInInspector]
		public int fingerID = 0;
		[Tooltip("If enabled, the distance between the touch and the pie option will be compared with OptionsStandardSize, to ensure that your finger in on the option.")]
		public bool checkIfFingerIsOnOption = false;
	}

	[Tooltip("Input that handles the Pie Menu")]
	public InputType inputType = InputType.Axis;
	[Tooltip("Options for the axis based input.")]
	public AxisOptions axisOptions;
	[Tooltip("Options for the mouse based input.")]
	public MouseOptions mouseOptions;
	[Tooltip("Options for the touch based input.")]
	public TouchOptions touchOptions;

	[Tooltip("Snap the center image to the nearest option.")]
	public bool SnapToOption = false;
	[Tooltip("If enabled, you will always have something selected.")]
	public bool SelectedIsNeverNull = true;
	[Tooltip("The central image never disappears.")]
	public bool AlwaysShowCenterImage = true;

	/// <summary>
	/// Slow motion slows down the game if the pie menu is active.
	/// </summary>
	[Tooltip("Slow motion slows down the game if the pie menu is active.")]
	public bool useSlowMotion = false;
	[Range(0,1)]
	public float SlowMotionTimeScale = 0.1f;
	
	[Tooltip("Shape of the Pie (or half-pie) menu.")]
	public Shape shape = Shape.Circle;

	[Range(0,360), Tooltip("Rotate the pie menu in degrees.")]
	public float RotationInDegrees = 0;

	[Tooltip("Distance from the Pie Center to the options.")]
	public float Radius = 150;
	[Tooltip("The center of the Pie Menu.")]
	public Vector2 Center = Vector2.zero;



	/// <summary>
	/// The text length will influence it's box's width this much.
	/// </summary>
	/// 
	[Range(0,10), Tooltip("Size of one option's box/image. Width = Height = OptionsStandardSize.")]
	public float TextWidthMultiplier = 5f;
	/// <summary>
	/// Size of one option's box/image. Width = Height = OptionsStandardSize.
	/// </summary>
	public float OptionsStandardSize = 50f;

	/// <summary>
	/// This image will always rotate towards the selected.
	/// </summary>
	[Tooltip("This image will always rotate towards the selected.")]
	public Texture2D CenterImage;

	/// <summary>
	/// This image will stay in the background and won't rotate.
	/// </summary>
	[Tooltip("This image will stay in the background and won't rotate.")]
	public Texture2D BackgroundImage;
	[Range(0,5)]
	public float CenterImageSizeMultiplier = 1f;
	[Range(0,5)]
	public float BackgroundImageSizeMultiplier = 2f;

	public GUISkin Skin;
	public string StyleNameForNormal = "Button";
	public string StyleNameForSelected = "Button";
	public Color CenterImageTint = Color.white;
	public Color SelectedOptionTint = Color.green;
	public float GlobalScale = 1f;

	private float currentRadius;
	private ExecutableOption[] Options;
	private bool showThePie = false;
	private bool Increase = false;
	private Matrix4x4 GUIBuffer;
	private float rotation = 0;
	private float prevrotation = 0;
	private int selectnum = -2;
	private int prevselectnum = -2;
	private bool changed = false;
	private System.Object PointerToDataArray;
	private bool frozen = false;

	/// <summary>
	/// Returns true if the Pie Menu is frozen. (It's not respondind and gets a grey tint)
	/// </summary>
	public bool Frozen {
		get {
			return frozen;
		}
	}

	/// <summary>
	/// Returns a pointer to the options array last time given. For example: this function is ideal if you want to get the current options set to compare it inside a statement.
	/// </summary>
	public System.Object MenuSet {
		get {
			return PointerToDataArray;
		}
	}

	/// <summary>
	/// Returns true if the Pie Menu is visible and active.
	/// </summary>
	public bool Active {
		get {
			return showThePie;
		}
	}

	/// <summary>
	/// Returns the index number of the Pie Menu selection.
	/// </summary>
	public int Selected {
		get {
			return selectnum;
		}
	}

	/// <summary>
	/// Returns the selected Object itself as GUIContent.
	/// </summary>
	public GUIContent SelectedObject {
		get {
			if (selectnum >= 0) return Options[selectnum].content;
			else return null;
		}
	}

	/// <summary>
	/// Returns the selected Object itself as ExecutableOption.
	/// </summary>
	public ExecutableOption SelectedExecutableOption {
		get {
			if (selectnum >= 0) return Options[selectnum];
			else return null;
		}
	}

	/// <summary>
	/// Returns true if the Pie Menu selection was changed.
	/// </summary>
	public bool Changed {
		get {
//			if (changed) {
//				changed = false;
//				return true;}
//			else return false;
			return changed;
		}
	}

	/// <summary>
	/// The touch ID used for the actual Pie menu.
	/// </summary>
	/// <value>The touch ID.</value>
	public int FingerID {
		get {
			return touchOptions.fingerID;
		}
		set {
			touchOptions.fingerID = Mathf.Clamp(value, 0, 9);
		}
	}

	/// <summary>
	/// Basic constructor. You don't need to create a new Pie Menu if it is defined public in a MonoBehaviour Environment. (Unity does it)
	/// </summary>
	public PieMenu () {
	}

	/// <summary>
	/// Initializes and Opens the Pie Menu with a nice transition.
	/// </summary>
	public PieMenu InitPie (GUIContent[] options)
	{
		PointerToDataArray = options;
		Options = new ExecutableOption[options.Length];
		for (int i = 0; i < Options.Length; i++) {
			Options[i] = new ExecutableOption(options[i]);
		}
		InitPie();
		return this;
	}

	/// <summary>
	/// Initializes and Opens the Pie Menu with a nice transition.
	/// </summary>
	public PieMenu InitPie (ExecutableOption[] options)
	{
		PointerToDataArray = options;
		Options = options;
		InitPie();
		return this;
	}

	/// <summary>
	/// Initializes and Opens the Pie Menu with a nice transition.
	/// </summary>
	public PieMenu InitPie (string[] options)
	{
		PointerToDataArray = options;
		Options = new ExecutableOption[options.Length];
		for (int i = 0; i < Options.Length; i++) {
			Options[i] = new ExecutableOption(options[i]);
		}
		InitPie();
		return this;
	}

	/// <summary>
	/// Inner function for finalizing the Initialization.
	/// </summary>
	void InitPie ()
	{
		showThePie = true;
		changed = false;
		if (axisOptions.HideCursor && inputType == InputType.Axis) {
			Screen.lockCursor = true;
			Screen.showCursor = false;
		}
		StartIt();
	}

	/// <summary>
	/// Returns true and opens the Pie Menu if it could be opened. Else returns false. Use this if you don't want to change the option array.
	/// </summary>
	public bool OpenPie () {
		if (Options != null) {
			InitPie();
			return true;
		}
		else return false;
	}

	bool transition = false;
	ExecutableOption[] transitionContent;

	/// <summary>
	/// Loads up the new option (GUIContent) array with a nice transition.
	/// </summary>
	public void TransitionPie (GUIContent[] options)
	{
		PointerToDataArray = options;
		StopIt();
		transition = true;
		transitionContent = new ExecutableOption[options.Length];
		for (int i = 0; i < transitionContent.Length; i++) {
			transitionContent[i] = new ExecutableOption(options[i]);
		}
	}

	/// <summary>
	/// Loads up the new option (GUIContent) array with a nice transition.
	/// </summary>
	public void TransitionPie (ExecutableOption[] options)
	{
		PointerToDataArray = options;
		StopIt();
		transition = true;
		transitionContent = options;
	}

	/// <summary>
	/// Loads up the new option (string) array with a nice transition.
	/// </summary>
	public void TransitionPie (string[] options)
	{
		PointerToDataArray = options;
		StopIt();
		transition = true;
		transitionContent = new ExecutableOption[options.Length];
		for (int i = 0; i < transitionContent.Length; i++) {
			transitionContent[i] = new ExecutableOption(options[i]);
		}
	}

	/// <summary>
	/// Returns the actual selected option and closes the Pie Menu.
	/// </summary>
	public int GetSelectedPieAndClose () {
		StopIt();
		if (axisOptions.HideCursor && inputType == InputType.Axis) {
			Screen.lockCursor = false;
			Screen.showCursor = true;
		}
		return selectnum;
	}

	/// <summary>
	/// Closes the Pie Menu.
	/// </summary>
	public void Close () {
		StopIt();
		if (axisOptions.HideCursor && inputType == InputType.Axis) {
			Screen.lockCursor = false;
			Screen.showCursor = true;
		}
	}

	/// <summary>
	/// Closes the Pie Menu, but does not unlock cursor. Useful for nested Pie Menus.
	/// </summary>
	public void Return () {
		StopIt();
	}

	/// <summary>
	/// Freezes the Pie Menu and changes it's tint color to gray, so it will look like a "Not responding window"
	/// </summary>
	public void Freeze (bool value) {
		frozen = value;
	}


	/// <summary>
	/// Returns true if the firstvalue (degree) is closer to the secondvalue (degree) than other values calculated by the amount of slices.
	/// </summary>
	bool isNearDegree (float circle,float firstvalue, float secondvalue, int amountofslices) {
		float OneSlice = circle / (amountofslices * 2);
		firstvalue = firstvalue % circle;
		secondvalue = secondvalue % circle;
		if (firstvalue < 0) firstvalue += circle;
		if (secondvalue < 0) secondvalue += circle;
		
		float diff = secondvalue - firstvalue;
		if (diff < 0) diff += circle;
		
		if ((diff >= circle - OneSlice) || (diff < 0 + OneSlice)) return true;
		else return false;
		
	}

	/// <summary>
	/// Inner helper function to show the pie.
	/// </summary>
	void StartIt () {
			Increase = true;
			if (useSlowMotion) Time.timeScale = SlowMotionTimeScale;
			rotation = 0; prevrotation = 0;
	}
	
	/// <summary>
	/// Inner helper function to hide the pie.
	/// </summary>
	void StopIt () {
			showThePie = false;
			Increase = false;
			if (useSlowMotion) Time.timeScale = 1f;
	}

	/// <summary>
	/// Function to clamp the angle, preventing it to get out from the given interval.
	/// </summary>
	float ClampAngle (float angle, float min, float max) {
		angle = NormalizeAngle(angle);
		min = NormalizeAngle(min);
		max = NormalizeAngle(max);
		if (min < max) return Mathf.Clamp (angle, min, max);
		else if (min == max) return max;
		else {
			if (angle > min - 90f) return Mathf.Clamp (angle, min, 360);
			else return Mathf.Clamp (angle, 0, max);
		}
	}

	/// <summary>
	/// Function to convert the angle to a more human readable value between 0 and 360.
	/// </summary>
	float NormalizeAngle (float angle) {
		if (angle < 0)
			angle += 360;
		if (angle > 360)
			angle -= 360;
		return angle;
	}


	/// <summary>
	/// Put this function inside the OnGUI function to run the Pie Menu wrapper.
	/// </summary>
	public void Run () {

		if (transition && currentRadius < 0) {
			Options = transitionContent;
			InitPie();
			transition = false;
		}

		if (showThePie){
			if ((Increase)&&(currentRadius < Radius)) {
				currentRadius = Mathf.Lerp(currentRadius, Radius, 0.1f);
			}

			DrawPie();
		}
		
		if (!(Increase) && (currentRadius > 0)) {
			currentRadius = Mathf.Lerp(currentRadius, -10f, 0.1f);
			DrawPie();
		}
		else if (!Increase) currentRadius = 0;
		
	}

	/// <summary>
	/// Here happens the magic.
	/// </summary>
	void DrawPie () {
		float X = 0, Y = 0;
		float itemradrot = 0;
		Vector2 CursorPos = Vector2.zero;
		if (Event.current.type == EventType.layout) changed = false;

		if (frozen) selectnum = -2;
		else if (inputType == InputType.Axis) {
			if (( Mathf.Abs(Input.GetAxis(axisOptions.XAxis)) > axisOptions.Treshold ) || ( Mathf.Abs(Input.GetAxis(axisOptions.YAxis)) > axisOptions.Treshold )) {
				//rotation = Mathf.Atan2(Input.GetAxis(XAxis)*5, Input.GetAxis(YAxis)*5) * Mathf.Rad2Deg;
				rotation = Mathf.Atan2(Input.GetAxis(axisOptions.XAxis), Input.GetAxis(axisOptions.YAxis)) * Mathf.Rad2Deg;
				selectnum = -1;
			}
			else if (!SelectedIsNeverNull) {
				selectnum = -2;
			}
			
			if (SnapToOption) {
				float oneOption = 0;
				if (shape == Shape.Semi_Circle) oneOption = ((Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				else if (shape == Shape.Circle) oneOption = ((2 * Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				if (rotation <= 0) rotation += 360;
				prevrotation = (Mathf.Round(rotation / oneOption) * oneOption) + (RotationInDegrees * Mathf.Deg2Rad);
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
			else {
				prevrotation = Mathf.LerpAngle(prevrotation,rotation, (Mathf.Abs(Input.GetAxis(axisOptions.XAxis)) + Mathf.Abs(Input.GetAxis(axisOptions.YAxis))) * axisOptions.Sensitivity);
				if (prevrotation >= 360) prevrotation -= 360;
				else if (prevrotation <= -360) prevrotation += 360;
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
		}
		else if (inputType == InputType.Mouse) {
			CursorPos = UseTouchPos(Input.mousePosition);
			rotation = Mathf.Atan2(CursorPos.x - Center.x , Center.y - CursorPos.y) * Mathf.Rad2Deg;

			float dist = Vector2.Distance(CursorPos, Center);
			if ((dist < (mouseOptions.innerInputRadius*GlobalScale)) || ((dist > (mouseOptions.outerInputRadius*GlobalScale)) && !SelectedIsNeverNull) ) selectnum = -2;
			else selectnum = -1;

			if (SnapToOption) {
				float oneOption = 0;
				if (shape == Shape.Semi_Circle) oneOption = ((Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				else if (shape == Shape.Circle) oneOption = ((2 * Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				if (rotation <= 0) rotation += 360;
				else if (rotation > 0) rotation -= 360;
				prevrotation = (Mathf.Round(rotation / oneOption) * oneOption) + (RotationInDegrees * Mathf.Deg2Rad);
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
			else {
				prevrotation = Mathf.LerpAngle(prevrotation,rotation, dist / (mouseOptions.innerInputRadius*GlobalScale));
				if (prevrotation >= 360) prevrotation -= 360;
				else if (prevrotation <= -360) prevrotation += 360;
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
		}
		else if (inputType == InputType.Touch) {
			float dist = 0;

			if (Input.touchCount > 0) {
				foreach (Touch t in Input.touches) {
					if (t.fingerId == FingerID) {
						CursorPos = UseTouchPos(t.position);

						rotation = Mathf.Atan2(CursorPos.x - Center.x , Center.y - CursorPos.y ) * Mathf.Rad2Deg;
						dist = Vector2.Distance(CursorPos, Center);
						break;
					}
				}
				selectnum = -2;
			}

			if ((dist < (touchOptions.innerInputRadius*GlobalScale)) || (Input.touchCount < 1) || ((dist > (touchOptions.outerInputRadius*GlobalScale)) && !SelectedIsNeverNull) ) selectnum = -2;
			else selectnum = -1;

			if (SnapToOption) {
				float oneOption = 0;
				if (shape == Shape.Semi_Circle) oneOption = ((Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				else if (shape == Shape.Circle) oneOption = ((2 * Mathf.PI)/ Options.Length) * Mathf.Rad2Deg;
				if (rotation <= 0) rotation += 360;
				else if (rotation > 0) rotation -= 360;
				prevrotation = (Mathf.Round(rotation / oneOption) * oneOption) + (RotationInDegrees * Mathf.Deg2Rad);
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
			else {
				prevrotation = Mathf.LerpAngle(prevrotation,rotation, dist / (touchOptions.innerInputRadius*GlobalScale));
				if (prevrotation >= 360) prevrotation -= 360;
				else if (prevrotation <= -360) prevrotation += 360;
				if (shape == Shape.Semi_Circle) prevrotation = ClampAngle(prevrotation, RotationInDegrees + 90, RotationInDegrees + 225);
			}
		}


		if (BackgroundImage != null)
		{
			GUI.DrawTexture( new Rect(Center.x - currentRadius*BackgroundImageSizeMultiplier*GlobalScale*0.5f*BackgroundImage.width*0.01f, Center.y - currentRadius*BackgroundImageSizeMultiplier*GlobalScale*0.5f*BackgroundImage.height*0.01f, currentRadius*BackgroundImageSizeMultiplier*GlobalScale*BackgroundImage.width*0.01f, currentRadius*BackgroundImageSizeMultiplier*GlobalScale*BackgroundImage.height*0.01f), BackgroundImage );
		}

		if (CenterImage != null && ((AlwaysShowCenterImage || SelectedIsNeverNull) || selectnum > -2))
		{
			GUIBuffer = GUI.matrix;
			GUIUtility.RotateAroundPivot(prevrotation , Center);
			GUI.color = CenterImageTint;
			GUI.DrawTexture(new Rect(Center.x - currentRadius*CenterImage.width*0.5f*CenterImageSizeMultiplier*GlobalScale*0.01f, Center.y - currentRadius*CenterImage.height*0.5f*CenterImageSizeMultiplier*GlobalScale*0.01f, currentRadius*CenterImage.width*CenterImageSizeMultiplier*GlobalScale*0.01f, currentRadius*CenterImage.height*CenterImageSizeMultiplier*GlobalScale*0.01f), CenterImage);
			GUI.color = Color.white;
			GUI.matrix = GUIBuffer;
		}

		if (Skin == null) Skin = ScriptableObject.CreateInstance<GUISkin>();

		GUIStyle normal = Skin.FindStyle(StyleNameForNormal);
		GUIStyle pressed = Skin.FindStyle(StyleNameForSelected);

		if (normal == null || pressed == null) {
			Debug.LogError("Pie Menu Error: The Style names are not valid.");
			normal = new GUIStyle ();
			pressed = new GUIStyle ();
		}

		if (currentRadius > OptionsStandardSize || Radius <= OptionsStandardSize) {
			GUIStyle final;

			for (int i=0; i < Options.Length; i++)
			{
				final = normal;
				
				if (shape == Shape.Semi_Circle) itemradrot = (((Mathf.PI)/ Options.Length) * i) + (RotationInDegrees * Mathf.Deg2Rad);
				else if (shape == Shape.Circle) itemradrot = (((2 * Mathf.PI)/ Options.Length) * i) + (RotationInDegrees * Mathf.Deg2Rad);
				
				X = (currentRadius * GlobalScale * Mathf.Cos(itemradrot) + Center.x );
				Y = (currentRadius * GlobalScale * Mathf.Sin(itemradrot) + Center.y );

				Rect act = new Rect(X - ((Options[i].text.Length * TextWidthMultiplier + OptionsStandardSize)*0.5f*GlobalScale),
				                    Y - OptionsStandardSize*0.5f*GlobalScale,
				                    Options[i].text.Length * TextWidthMultiplier + OptionsStandardSize*GlobalScale,
				                    OptionsStandardSize*GlobalScale);

				if	(isNearDegree((shape == Shape.Circle) ? 360 : 180 ,prevrotation,(itemradrot * Mathf.Rad2Deg)+90,Options.Length) && ((selectnum > -2) || SelectedIsNeverNull) &&  IsCursorNearAnOption(CursorPos, act.center, act.width) ) {
					if (Event.current.type == EventType.layout) {
						if (prevselectnum != i && (selectnum > -2) && currentRadius >= Radius/2f) changed = true;
						else if (i == prevselectnum || (selectnum < -1)) changed = false;
					}
					selectnum = prevselectnum = i;
					GUI.color = SelectedOptionTint;
					final = pressed;
				}
				
				if (frozen) GUI.color = Color.gray;
				GUI.Box( act, Options[i].content, final);
				GUI.color = Color.white;
			}
		}
	}

	/// <summary>
	/// Determines whether the cursor is near an option.
	/// </summary>
	/// <returns><c>true</c> if the cursor is near an option otherwise, <c>false</c>.</returns>
	/// <param name="cursorPos">Cursor position.</param>
	/// <param name="optionPos">Option position.</param>
	/// <param name="range">Range.</param>
	private bool IsCursorNearAnOption(Vector2 cursorPos, Vector2 optionPos, float range) {
		if (SelectedIsNeverNull) return true;
		else if (inputType == InputType.Mouse && mouseOptions.checkIfCursorIsOnOption) return (Vector2.Distance(cursorPos, optionPos) < range);
		else if (inputType == InputType.Touch && touchOptions.checkIfFingerIsOnOption) return (Vector2.Distance(cursorPos, optionPos) < range);
		else return true;
	}

	/// <summary>
	/// Execute the actual selection.
	/// </summary>
	public void Execute () {
		if (SelectedExecutableOption != null && SelectedExecutableOption.isExecutable) {
			try {
				SelectedExecutableOption.SendMessageTarget.SendMessage(SelectedExecutableOption.MethodName, SelectedExecutableOption.MethodArgument, SendMessageOptions.DontRequireReceiver);
			}
			catch (System.MissingMethodException mme) {
				Debug.LogError("Pie Menu Error: The argument \"" + SelectedExecutableOption.MethodArgument + "\" given at \"" + SelectedExecutableOption.ElementName + "\" is incompatible with the target method: \""
				               + SelectedExecutableOption.MethodName + "\".\nAdditional information:\n" + mme.Message );
			}
		}
		else if (SelectedExecutableOption == null) return;
		else Debug.LogWarning("The selected option cannot be executed.");
	}

	/// <summary>
	/// Inverts the screen's y axis. Unity uses the top-left corner of the screen as origo for GUI, but buttom-left for the input.
	/// </summary>
	/// <returns>The y inverted touch position.</returns>
	/// <param name="touchPos">Touch position.</param>
	public static Vector2 UseTouchPos(Vector2 touchPos) {
		Vector2 newTouchPos = touchPos;
		newTouchPos.y = Screen.height - touchPos.y;
		return newTouchPos;
	}
}

[System.Serializable]
public class ExecutableOption {
	public string ElementName;
	public GUIContent content;
	public Component SendMessageTarget;
	public string MethodName;
	public Object MethodArgument;

	public string text {
		get {
			return content.text;
		}
		set {
			content.text = value;
		}
	}

	public Texture image {
		get {
			return content.image;
		}
		set {
			content.image = value;
		}
	}

	public string tooltip {
		get {
			return content.tooltip;
		}
		set {
			content.tooltip = value;
		}
	}

	public bool isExecutable {
		get {
			if (SendMessageTarget != null && !string.IsNullOrEmpty(MethodName)) return true;
			else return false;
		}
	}

	public ExecutableOption () {}

	public ExecutableOption (string text) {
		this.content = new GUIContent(text);
	}

	public ExecutableOption (GUIContent guicontent) {
		this.content = guicontent;
	}

	public ExecutableOption (GUIContent guicontent, Component target, string message) {
		this.content = guicontent;
		this.SendMessageTarget = target;
		this.MethodName = message;
	}

	public ExecutableOption (GUIContent guicontent, Component target, string message, Object argument) {
		this.content = guicontent;
		this.SendMessageTarget = target;
		this.MethodName = message;
		this.MethodArgument = argument;
	}
}

public static class PieUtility {

	/// <summary>
	/// Check if there is a touch with the specified fingerID.
	/// </summary>
	/// <param name="touches">Touch array.</param>
	/// <param name="fingerID">Finger ID.</param>
	public static bool Exists (this Touch[] touches, int fingerID) {
		for (int i = 0; i < touches.Length; i++) {
			if (touches[i].fingerId == fingerID) return true;
		}
		return false;
	}

	/// <summary>
	/// Find a specific touch, by fingerID.
	/// </summary>
	/// <param name="touches">Touch array.</param>
	/// <param name="fingerID">Finger ID.</param>
	/// <param name="found">True if found.</param>
	public static Touch Find (this Touch[] touches, int fingerID, out bool found) {
		for (int i = 0; i < touches.Length; i++) {
			if (touches[i].fingerId == fingerID) {
				found = true;
				return touches[i];
			}
		}
		found = false;
		return new Touch();
	}
}