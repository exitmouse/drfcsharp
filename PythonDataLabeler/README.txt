This folder contains our DataLabeler program in ./src/DataLabeler.py.

/******************************************************************************
 * Rolls:								      *
 ******************************************************************************/

The DataLabeler program has several uses:
1) We used the DataLabeler program as a gui to collect our labelings of the 
aerial images in our dataset.
2) We use the DataLabeler as a viewer for the classification labeling created
by running out model on our aerial data set, as well as for viewing Kumar and 
Hebert's labelings of their data, and classifications of that data.
3) Finally, the DataLabeler is the program we use to get quantitative analysis 
of classifications.



/******************************************************************************
 * Dependancies:							      *
 ******************************************************************************/

DataLabeler is written in python, but it has several dependancies beyond the 
python standard libraries.  These dependancies are:
*Python Image Library
*pygame (for gui elements)
*numpy



/******************************************************************************
 * Usage:							      	      *
 ******************************************************************************/

The available commands are:

Navigation:
    esc to exit
    d to go to the next image
    a to go to the previous image
    g to enter goto mode (in which to type the index of an image to goto), and 
	g again to actually go there
    o to toggle between labeling images and viewing classifications
    l to toggle between our data set (aerial images) and Kumar and Hebert's 
    	data set.

Editing labelings (disabled except for labeling mode on aerial photos):
    left click to label a site as 1 (a building)
    shift left click to label a site as 2 (a non-building structure)
    right click to label a site as 0
    b to increment every labeling of the current image modulo 3

Running the quantitative analysis:
    u to run a quantitative analysis of the classification outputs for the
	current dataset


Notes: All labelings are immediately saved to the corresponding data file (a comma separated file representing the site labeling array), so there is no need for a save command.  Whenever an image is loaded for whom a labeling has already been saved, the program will automatically load that saved labeling.




