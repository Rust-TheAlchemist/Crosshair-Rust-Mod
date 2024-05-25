Initial modification of MisterPixie's UMOD for Rust called Crosshair v. 3.1.0.
https://umod.org/community/crosshair

5/25/2024
Uploaded Crosshair.cs and started version number back at 1.0.0.

This is a complete overhaul of MisterPixie's v. 3.1.0 that now allows for customization of the crosshair's color, text, and whether the user wants it to be auto-enabled.

5/17/2024
I'm updating the version to 3.1.1.

Added auto-enabling on player connect and player respawn. Yeah, not ideal if a player doesn't want it.
I'll add capability later to save a player's preference to whether it should be on or off. One step at a time.

After that, the plan is to also make the position of the crosshair editable by the server admin via a config file.
Then, make it editable on a per player basis.

Maybe after that, allow them to actually change the crosshair look itself (though it's currently a text character, not an image, so, that will take some work).
