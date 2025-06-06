using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Display a temporarily notification on top of screen during gameplay
/// </summary>
[EntityAttribute.Capacity(6, 0)]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.DontDespawnOutOfRange]
public class NotificationUI : EntityUI {

	// VAR
	private static readonly int TYPE_ID = typeof(NotificationUI).AngeHash();
	private static int CurrentNotificationCount = 0;
	private string Content = "";
	private int Icon = 0;
	private int NotificationIndex = 0;
	private int CurrentOffsetY = 0;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		CurrentOffsetY = 0;
	}

	public override void BeforeUpdate () {
		base.BeforeUpdate();
		CurrentNotificationCount = 0;
	}

	public override void Update () {
		base.Update();
		NotificationIndex = CurrentNotificationCount;
		CurrentNotificationCount++;
	}

	public override void UpdateUI () {

		base.UpdateUI();

		const int EASE_DURATION = 30;
		const int STAY_DURATION = 300;

		int localFrame = Game.GlobalFrame - SpawnFrame;
		if (localFrame > STAY_DURATION) {
			Active = false;
			return;
		}
		int panelWidth = Unify(500);
		int panelHeight = Unify(42);
		CurrentOffsetY = CurrentOffsetY.LerpTo((panelHeight + Unify(30)) * NotificationIndex, 60);
		var fromRect = new IRect(
			Renderer.CameraRect.CenterX() - panelWidth / 2,
			Renderer.CameraRect.yMax,
			panelWidth, panelHeight
		);
		var toRect = new IRect(
			Renderer.CameraRect.CenterX() - panelWidth / 2,
			Renderer.CameraRect.yMax - panelHeight - Unify(24) - CurrentOffsetY,
			panelWidth, panelHeight
		);
		var panelRect =
			localFrame < EASE_DURATION ? fromRect.LerpTo(toRect, Ease.OutBack((float)localFrame / EASE_DURATION)) :
			localFrame > STAY_DURATION - EASE_DURATION ? fromRect.LerpTo(toRect, Ease.OutBack((float)(STAY_DURATION - localFrame) / EASE_DURATION)) :
			toRect;

		// BG
		Renderer.DrawPixel(panelRect.Expand(Unify(12)), Color32.BLACK, int.MaxValue - 1);

		// Icon
		if (Renderer.TryGetSprite(Icon, out var icon, false)) {
			Renderer.Draw(
				Icon,
				new IRect(panelRect.x, panelRect.y, panelRect.height, panelRect.height).Fit(icon),
				int.MaxValue
			);
		}

		// Label
		GUI.Label(
			panelRect.Shrink(panelRect.height + Unify(12), 0, 0, 0),
			Content, GUI.Skin.CenterLabel
		);

	}

	/// <summary>
	/// Require a notification. Call this function once for a single notification.
	/// </summary>
	/// <param name="content">Text content of the notification</param>
	/// <param name="icon">Artwork sprite ID for the notification. Set to int.MinValue if no icon should be display.</param>
	public static void SpawnNotification (string content, int icon = int.MinValue) {
		if (Stage.SpawnEntity(TYPE_ID, 0, 0) is not NotificationUI ui) return;
		ui.Content = content;
		ui.Icon = icon == int.MinValue ? BuiltInSprite.ICON_INFO : icon;
	}

}