# Virtual Robot Interaction with AR (HoloLens)
<video width="640" height="360" controls>
  <source src= "https://youtu.be/mQSO31yORGY" type="video/mp4">
  Your browser does not support the video tag.
</video>
This project is being developed as part of the course "Robotics Perception and Action" held at the University of Trento, Italy.
It consists of an app that can be installed on Microsoft Hololens, an augmented reality visor. The software is written in C# and Unity is the motor. In particular, the Mixed Reality Toolkit was used to allow the user to interact with the program.

When starting the app, the user has to scan the room to identify obstacles. Once this is done, the user can place the cube, representing the goal that the robot must achieve, in any place of the grid that is automatically generated. The robot is able to compute a trajectory, using the A* algorithm, and reach the target without colliding with the obstacles.




## Authors

- [Ciresa Simone](https://www.github.com/octokatherine)
- Valeria Grotto
- Guglielmo Del Col
- Iga Pawlak 


## Control Panel
The Control Panel is the main interface for the user. By following the steps and activating the corresponding buttons, the user is able to complete the game and the robot is able to reach the target.
![Control Panel](https://github.com/ciresimo/Mixed_Reality_Differential_Drive/blob/main/ControlPanel.jpg)

## A* Algorithm
After scanning the room, a grid is automatically generated. The space surrounding the user is divided into cells of the same dimension. Small cubes will appear, indicating whether the cell is free or not. Once the end location marker is placed in the desired position, a new path can be computed. The Algorithm implemented is A*, shown in the following image

## Differential Drive Vehicle’s Kinematic
Each wheel has a linear velocity  𝑣_R = 𝜔_𝑅*r and 𝑣_L = 𝜔_L*r. From these values, the linear velocity of the robot can be computed as
𝑣 = r * (𝜔_𝑅-𝜔_L) /d                                                       

A difference in the velocity of the two wheels, instead, generates a rotation of the vehicle around a point lying on the wheel axis, called the Instantaneous Center of Curvature (ICC).

## Pure Pursuit Control
The trajectory control we used is a Pure Pursuit algorithm that geometrically determines the curvature that will drive the vehicle to a goal path point. We iteratively calculate the ICC (instantaneous centre of curvature) between the centre of the robot and the intersection between the lookahead distance and the path that we have to follow. We then use the angle calculated to pass the correct torques to the wheels of our robot.
Using ICC we guarantee a smoother path of the robot even in sharp curves. 
If the robot is moved from the path that it is trying to follow, the new target point will become the final point of the segment of the path that it wants to follow. When the robot intersects the path again, the target point will be the lookahead point, as explained in the figure.


