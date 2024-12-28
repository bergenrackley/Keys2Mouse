# Keys2Mouse

This is a quick and dirty attempt at creating a Windows version of croian's Mouseless application, as it currently only has a Mac build

https://github.com/croian | https://www.youtube.com/watch?v=J0rwQVNQkHM | https://mouseless.click/

Please support the Mouseless project, it is surely much more polished than mine.

# Installation
There is no installation process, just download the release zip and run the Keys2Mouse exe. Make sure it's in a directory you plan to keep it in before changing the RunOnStart setting to true.

# Settings
To change settings, open the System Tray and double click on the app icon. Alternatively, press "?" while the overlay is open.

# Usage
To **summon** the overlay, press Win + LCtrl.

Type the letters of a cell to **move your mouse to the cell**, and press the next key for the subgrid.

To **left click**, press LCtrl. To **double click**, hold 2 and press LCtrl. To **triple click**, hold 3 and press LCtrl.

To **right click**, press Tab.

To **enter text select mode**, press LShift+LCtrl. If you have previously specified a cell while the overlay was opened, this will be used as the start position. If not, you will need to enter two grid cell inputs. Once the second input has been recieved, the text will be highlighted and the overlay will close (this sometimes seems to have issues with dragging text around, I've tried to fix this by adding some delay between clicking and moving which seems to help, but let me know if there's any issues). The state of the overlay is indicated by the background color, which can be configured in the settings menu. By default, Green means Coord1, Blue means Coord2.

To **close** the dialog, either press Esc or the summon shortcut again (Win+LCtrl).

To **reset** the dialog, press backspace.

To open the **settings** menu, press "?".

All actions besides summoning the overlay must be performed while the overlay is open, the only HotKey the app registers is Win+LCtrl.

If the key combinations above don't seem to be working while the overlay is opened, you may have accidentally focused another window while summoning the overlay. Close the overlay with Win+LCtrl and resummon it.

# Attributions
As mentioned above, the application itself is croian's, I just wanted a version that works on Windows. Check his work out here: https://github.com/croian

<a href="https://www.flaticon.com/free-icons/computer" title="computer icons">App icon created by Atif Arshad - Flaticon</a>
