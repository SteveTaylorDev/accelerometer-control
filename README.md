# Accelerometer Control

Android project built in Unity utilizing the accelerometer as input to manipulate the scene.

Controller scripts for the Game, Camera and Gravity manage their respective tasks.

The Game Controller handles player behaviour like setting its orientation with gravity and applying it, and using Unity's collision events to apply friction force when in contact with a wall. A global 'Current Down Vector' is set, which represents the current world down direction.

The Camera Controller features options that allow, among other things, offsetting the current focal target and adjusting zoom by a certain amount depending on the player's current velocity.

Using the Input Manager through Unity's scripting API, the accelerometer can be read as X and Y inputs. This can be combined into a Vector3. which is then fed into the Camera and Gravity controllers, both of which have multiple useage options for this vector built into them.

This allows for customizable tools that allow the manipulation of the gravity of the scene, as well as the camera orientation, based on phone rotation.

A seperate mode for regular input is included, allowing camera and gravity rotation when running on a device with no accelerometer. 
