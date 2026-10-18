# Steam Input

Xbox pad: no layout change. The real D-pad already sends D-pad up and down.

Steam Deck: import or apply `deck-look.vdf`, or bind it by hand. Controller settings → Hollow Knight → Edit Layout → back buttons / grip → L4 = D-pad Up, R4 = D-pad Down. L5 and R5 are left alone.

`deck-look.vdf` is a Deck (`controller_neptune`) layout shipped in this repo. It is not published on the Steam Community. It is the stock Deck gamepad template plus two grip aliases: L4 sends D-pad Up and R4 sends D-pad Down. In the file those inputs are `button_back_left_upper` and `button_back_right_upper`. Left stick, right stick, A/B/X/Y, bumpers, triggers, Start, Back, and the D-pad stay on their usual gamepad outputs. Attack stays on Xbox X.

This only aliases the buttons. The mod does the look. Applying the layout without the mod makes L4 and R4 press D-pad up and down, which in vanilla Hollow Knight is movement, not instant look.

## Apply the file

Quit Steam first so it does not overwrite the config when it exits.

Copy `deck-look.vdf` onto the Hollow Knight Deck slot (app id 367520):

`~/.local/share/Steam/steamapps/common/Steam Controller Configs/<SteamID>/config/367520/controller_neptune.vdf`

In `config/configset_controller_neptune.vdf`, point `367520` at that file instead of the stock gamepad template:

```
"367520"
{
	"autosave"		"1"
}
```

Start Steam. You can also copy `deck-look.vdf` into `~/.local/share/Steam/controller_base/templates/` and choose the template named "C-Stick look on grips".
