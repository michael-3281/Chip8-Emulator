CHIP-8 EMULATOR

**Version:** 1.1.0

**Platforms:** Windows (x64), Web (Blazor WebAssembly)

**Author:** Michael 

**What is CHIP-8?**



CHIP-8 is a simple, interpreted programming language developed in
the mid- 1970s by Joseph Weisbecker. It was designed to make it
easier to write games for early microcomputers like the COSMAC VIP
and Telmac 1800.

Rather than being a real hardware system, CHIP-8 is a virtual
machine specification. Many of the classic "retro" games you see
today--Pong, Tetris, Space Invaders, Breakout--were written for
CHIP-8.

This emulator recreates that virtual machine in software so those
original ROMs (read only memory) can be run on modern systems.



**Ways to Play**



1. Windows Desktop (Standalone)

* **Download** - Get the 'v1.1.0' ZIP from the [Releases](https://github.com/michael-3281/Chip8-Emulator/releases) page.
* **Run**      - Extract the folder and double-click 'CHIP8Emulator.exe'
* **Note**     - Ensure the 'roms' folder stays next to the '.exe'.



2\. Web Version (Self-Build)

**Hosted Link:** [Here](https://michael-3281.github.io/Chip8-Emulator/)
* The source code for the Blazor WebAssembly version is located in '/WebAssembly'.



**Project Structure**

This repository contains the complete source code for both versions of the emulator:

**/Chip8Emulator:** The Windows Forms desktop application

**/WebAssembly:**   The Blazor WebAssembly web application.

**/roms:**          A collection of classic CHIP-8 games.

**CHIP-8.sln:**     The master solution file to build both projects.





**CHIP-8 Technical Specifications**



**Memory**

* 4 KiB (4096 bytes) total
* Programs start at memory address 0x200 (512)



**Display**

* Resolution: 64 x 32 pixels
* Monochrome (black \& white)
* Pixels are either ON or OFF



**Registers**

* 16 general-purpose 8-bit registers: V0-VF
* VF is used as a flag register (carry, collision, etc.)



**Index Register**

* I (16-bit)



**Stack**

* 16 levels
* Used for subroutine calls



**Timers**

* Delay timer (60 Hz)
* Sound timer (64 Hz, beeps when nonzero)



**Input**

* 16-key hexadecimal keypad



**Emulator Features**



* Accurate CHIP-8 instruction set
* Built-in ROM launcher
* Keyboard input mapping
* Real-time rendering
* Collision detection
* Sprite wrapping
* Sound timer beep
* Page-based ROM selector


**How to Add ROMs**

1. Download CHIP-8 ROMs (.ch8 files, .txt untested)
2. Drop them into the roms folder
3. Restart the emulator (close and relaunch)
4. They will appear automatically in the launcher



**Launcher Controls**



**Key**	    **Action**

W	    Move up

S	    Move down

Enter / E   Select ROM

Escape	    Return to launcher



**CHIP-8 Keymap Layout**



CHIP-8 uses a 16-key hexadecimal keypad:



1 2 3 C

4 5 6 D

7 8 9 E

A 0 B F



**Keyboard Mapping**



CHIP-8	Keyboard

1	1

2	2

3	3

C	4

4	Q

5	W

6	E

D	R

7	A

8	S

9	D

E	F

A	Z

0	X

B	C

F	V



NOTICE to users:

Game keybinds can be strange, and are unfortunately unable to be changed,

however all have been found to work, so if the game is

not responding to input, please try a different key.



**Rendering**

* Display is scaled 10x for visibility
* Each CHIP-8 pixel is drawn as a 10x10 square
* Black = OFF, White = ON



**Timing**

* CPU cycles: Multiple per frame
* Timers tick at ~60 Hz
* Sound timer beeps when reaching 1



**Known Limitations**

This emulator focuses on standard CHIP-8 compatibility



**Not Implemented (Yet)**

* Super CHIP-8 instructions
* High-resolution modes
* Save states
* Audio waveforms
* Speed control
* Debugger



NOTICE: Some ROMs written for Super CHIP-8 or XO-CHIP may not

work correctly.



**Troubleshooting**



**"NO ROMS FOUND"**

* Ensure the roms folder is next to the .exe
* Ensure ROMs use the .ch8 extension
* Restart the emulator



**Game runs too fast/slow**

* Timing is fixed internally (no speed control)



**No sound**

* Beep is used for sound
* Ensure your system sound is enabled



**Windows Defender**

* Because this app is not digitally signed, Windows may show a warning.
* Click "**More Info"** and then **"Run Anyway"** to start the program.



**"Loading..." screen stuck on web:** 

Perform a **Hard Refresh** (Ctrl + F5) or (Cmd + Shift + R)



**Version History**



**v1.0.0**

* Initial release
* ROM launcher
* Full instruction set
* Keyboard input
* Sound support



**v1.0.1**

* Fixed pathing bug which caused ROMs to be unable to be found



**V1.1.0**

* Multi-Platform Support: Integrated Blazor WebAssembly project for



**V1.2.0 (Current)**

* Public Hosting: Successfully deployed the WebAssembly version to GitHub Pages. 

browser-based emulation

* Unified Project Structure: Refactored repository to include both

Desktop and Web source code under a single solution

* Improved Organization: Added centralized roms directory and .gitignore

for cleaner repository management



**Credits**

* CHIP-8 originally designed by Joseph Weisbecker
* Emulator implementation by Michael
* ROMs written by their respective creators.



**License**

This project is released under the **MIT License.** See the LICENSE file

for details.



**Enjoy retro computing!**

* Future emulators to come




