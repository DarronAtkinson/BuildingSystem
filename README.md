# BuildingSystem

Core scripts used for an in-game building system I am working on. </br>
</br>
Very much a work in progress.</br>
</br>

# How it works

### Support Point
A support point defines a point on a structure that either requires support or provides outgoing support.

### Structure
A structure is an in game representation of a building element. e.g foundation, post, beam etc.</br>
A structure must have support at it's required support points or else it will collapse.

### Structure Preview
A structure preview also a structure to be visualised and positioned in the world.</br>
A structure can only be placed if the supports it requires are present in the world.

### Support
A support maintains a heap of structures and the support value they provide at that position</br>
The registered structures are alerted to any changes made to the support value and react accordingly

### Build Manager
A build manager maintains an octree of all the supports created by a player.</br>
This restricts the player to only placing structures on their own buildings

## Example Video.
https://www.youtube.com/watch?v=jgyNRN919w4

# Todo

Snapping structures together at specific support points while previewing a structure needs work. </br>
Visual feedback for the player on whether the placement of a structure preview is valid. </br>
Saving and loading of structures and support points needs to be implemented. </br>
