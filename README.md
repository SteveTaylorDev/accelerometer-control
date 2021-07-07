# Accelerometer Control

Wanting to create a project for smartphones, I had an idea utilizing the accelerometer as input to manipulate physics properties.

Controller scripts for the Game, Camera and Gravity manage their respective tasks.

The Game Controller handles player behaviour like setting its orientation with Gravity and applying it, and using Unity's collision events to apply friction force when in contact with a wall. A global 'Current Down Vector' is set, which represent the current world down direction.

Using the Input Manager through Unity's scripting API, accelerometer x and y inputs are read and combined into a vector3. This vector can then be fed into the controllers, which have multiple useage options for this vector built into them.

This allows for customizable tools that allow the manipulation of the gravity of the scene, as well as the camera orientation, based on phone rotation.

A seperate mode for regular input is included, allowing camera and gravity rotation when running on a device with no accelerometer. 
