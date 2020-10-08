# CSVIO
An easy solution for creating, modifying and instantiating various objects made out of smaller components.

Purpose: 
In order to create a game, you need to create many objects, and hold a lot of data. Once you define objects a certain way, it is often too cumbersome to modify them, check their well being, or write them out. This project aims to limit the amount of time developers need to spend creating, loading and modifying objects so that they can focus on creating the mechanics of their games and tell the stories they want to tell. In essence, it makes reading and writing between Unity and CSV's really simple. 

How the project works:
The main script in this package is (CSVIOTestProject/Assets/Scripts/CSVIO.cs CSVIO.cs) which defines the classes CSVIO and CompObj (short for Composite Object).
Simply put, a CompObj is an Object created from specified properties of a simpler nature (ex. strings, ints, booleans and smaller CompObjs). No matter how complicated the Object, once defined, this project can find a way to non-destructively store all the data that comprises the object. This leaves the user able to modify and use the object to their hearts content.

What's next?
 -> Currently, the basic objects with which to build Composite objects is limited to strings, ints and bools. In the future, I would like this to include more predefined classes such as Images.
 -> The use of System.Reflection may pose a problem when dealing with large amounts of data, like location maps and other big multidimensional arrays. While this works well during the development of the game, it may become better to have a FastOutput() function which does not need to use System.Reflection once the developement of the game is complete. The FastOutput option is still on the table for the future.


