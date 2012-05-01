'''
Created on Apr 29, 2012

@author: Dan Denton and Jesse Selover


This is our utility program for labeling photograph sites.  The commands are:
    esc to exit
    d to go to the next image
    a to go to the previous image
    left click to label a site as 1
    shift left click to label a site as 2
    right click to label a site as 3
All labelings are immediately saved to the corresponding data file (a comma separated
file representing the site labeling array), so there is no need for a save command.
Whenever an image is loaded for whom a labeling has already been saved, the program
will automatically load that saved labeling.
'''

import os, sys
import numpy
import Image
import pygame
from pygame.locals import *
import csv

COLOR_WHITISH = (200, 200 , 200)
COLOR_BLACK = (0, 0 , 0)    
COLOR_BLUISH = (200, 200 , 250)
COLOR_ROSE = (250, 200 , 200)
SITE_INDICATOR_OVERLAY_ALPHA = 100

LEFT_BUTTON = 1
RIGHT_BUTTON = 3

IMAGE_ZOOM = 2
IMAGE_DIM = 256

SITE_DIM = 16

DATA_LABELER_DIR = os.path.split(os.path.abspath(__file__))[0]
DATASET_DIR = os.path.normpath(os.path.join(DATA_LABELER_DIR, '..\..\Dataset'))
NUM_IMAGES = 300



def LoadImage(name):
    fullname = os.path.join(DATASET_DIR, name)
    try:
        image = pygame.image.load(fullname)
    except pygame.error, message:
        print 'Cannot load image: ', name
        raise SystemExit, message
    image = image.convert()
    return image, image.get_rect()



class Background(pygame.sprite.Sprite):
    """moves a clenched fist on the screen, following the mouse"""
    def __init__(self):
        pygame.sprite.Sprite.__init__(self) #call Sprite initializer


        
''' A method to load the currently saved labeling of the indicated data
file from a comma separated text file named <image_index>.txt, located
in the Dataset directory.  This method also creates and returns the 
background sprite for the image.
'''
def LoadAerial(image_index, site_array):
    current_image_index_str = str(image_index).zfill(3)
    current_image_name = current_image_index_str + '.jpg'
    current_image_name2 = current_image_index_str + '.jpeg'
    current_data_name = current_image_index_str + '.txt'
    image_filename = os.path.join(DATASET_DIR, current_image_name)
    data_filename = os.path.join(DATASET_DIR, current_data_name)
    
    
    ''' This is just temporary code to give us a cropped version of the image.
    Eventually, we will want to pre-crop and pre-rotate all of the images in
    our data set. '''
    im = Image.open(image_filename) #@UndefinedVariable
    box = (0, 0, 0 + IMAGE_DIM, 0 + IMAGE_DIM)
    im = im.crop(box)
    im = im.resize((IMAGE_ZOOM * IMAGE_DIM, IMAGE_ZOOM * IMAGE_DIM))
    savename = os.path.join(DATASET_DIR, 'tmp.jpg')
    im.save(savename)
    
    
    ''' zero the site array
    '''
    for j in range(len(site_array)):
        for i in range(len(site_array)):
            site_array[i][j] = 0
    
    ''' load saved data into the site array if applicable
    '''
    try:
        f = open(data_filename, "r")
        try:
            doc = f.read() 
            print "Loading from: " + data_filename
            reader = csv.reader(doc)
            i = 0
            j = 0
            for row in reader:
                if len(row) == 0:
                    i = 0
                    j += 1
                elif len(row) == 1:
                    site_array[i][j] = int(row[0])
                    i += 1
        finally:
            f.close()
    except IOError:
        print "no open"
        pass
    
    background = Background()
    background.image, background.rect = LoadImage('tmp.jpg')
    background.scale = 2.0
    return background



''' A method to save the current values of the site array to a comma separated
text file named <image_index>.txt, located in the Dataset directory.
'''
def SaveAerial(image_index, site_array):
    current_data_index_str = str(image_index).zfill(3)
    current_data_name = current_data_index_str + '.txt'
    image_data_filename = os.path.join(DATASET_DIR, current_data_name)
    
    print image_data_filename
    
    try:
        f = open(image_data_filename, "w")
        try:
            for j in range(len(site_array)):
                line = ''
                for i in range(len(site_array)):
                    line = line + ('%d,' % site_array[i][j])
                line = line + '\n'
                f.write(line)
        finally:
            f.close()
    except IOError:
        print "no open"
        pass
    


def ScreenCoordsToSiteCoords (coords):
    assert len(coords) == 2
    assert 0 <= coords[0] and coords[0] < (IMAGE_ZOOM * IMAGE_DIM)
    assert 0 <= coords[1] and coords[1] < (IMAGE_ZOOM * IMAGE_DIM)
    
    x = coords[0] / (IMAGE_ZOOM * SITE_DIM)
    y = coords[1] / (IMAGE_ZOOM * SITE_DIM)
    
    return (x, y)


def SiteCoordsToScreenCoords (coords):
    assert len(coords) == 2
    assert 0 <= coords[0]
    assert 0 <= coords[1]
    
    x = coords[0] * (IMAGE_ZOOM * SITE_DIM)
    y = coords[1] * (IMAGE_ZOOM * SITE_DIM)
    
    assert x < (IMAGE_ZOOM * IMAGE_DIM)
    assert y < (IMAGE_ZOOM * IMAGE_DIM)
    
    return (x, y)



def main(index):
    ''' Initialize the current image index.'''
    current_image_index = index
    
    
    assert IMAGE_DIM % SITE_DIM == 0, "We require that the sizes evenly partition the image." 
    site_array_dim = IMAGE_DIM / SITE_DIM
    site_array = numpy.zeros((site_array_dim, site_array_dim), dtype=numpy.int)
    
    pygame.init()
    screen = pygame.display.set_mode((IMAGE_ZOOM * IMAGE_DIM, IMAGE_ZOOM * IMAGE_DIM))
    pygame.display.set_caption('DataLabeler by Dan Denton and Jesse Selover (%d)' % current_image_index)
    
    background = LoadAerial(current_image_index, site_array)
    allsprites = pygame.sprite.RenderPlain((background))
    
    ''' Draw the grid overlay surface
    '''
    grid = pygame.Surface(screen.get_size())
    grid = grid.convert()
    grid.fill(COLOR_BLACK)
    grid.set_colorkey(COLOR_BLACK, pygame.RLEACCEL)
    for i in range(1, site_array_dim):
        pygame.draw.line(grid, COLOR_WHITISH, (0, i * (SITE_DIM * IMAGE_ZOOM)), (IMAGE_DIM * IMAGE_ZOOM, i * (SITE_DIM * IMAGE_ZOOM)))
        pygame.draw.line(grid, COLOR_WHITISH, (i * (SITE_DIM * IMAGE_ZOOM), 0), (i * (SITE_DIM * IMAGE_ZOOM), IMAGE_DIM * IMAGE_ZOOM))       
    
    
    '''' Draw the site indicator overlay boxes
    '''
    box1 = pygame.Surface((SITE_DIM * IMAGE_ZOOM, SITE_DIM * IMAGE_ZOOM))
    box1.fill(COLOR_BLUISH)
    box1.set_alpha(SITE_INDICATOR_OVERLAY_ALPHA, pygame.RLEACCEL)
    box1 = box1.convert_alpha()
    
    box2 = pygame.Surface((SITE_DIM * IMAGE_ZOOM, SITE_DIM * IMAGE_ZOOM))
    box2.fill(COLOR_ROSE)
    box2.set_alpha(SITE_INDICATOR_OVERLAY_ALPHA, pygame.RLEACCEL)
    box2 = box2.convert_alpha()
    
    
    
    ''' Main loop
    ***************************************************************************
    '''
    clock = pygame.time.Clock()
    while True:
        clock.tick(60)
        
        for event in pygame.event.get():
            if event.type == QUIT:
                return
            
            
            elif event.type == KEYDOWN:
                '''
                Keyboard controls are:
                    esc to exit the labeler program
                    d to go to the next image
                    a to go to the previous image
                '''
                if event.key == K_ESCAPE:
                    return
                elif event.key == K_d:
                    current_image_index = (current_image_index + 1) % NUM_IMAGES
                    background = LoadAerial(current_image_index, site_array)
                    allsprites = pygame.sprite.RenderPlain((background))
                    pygame.display.set_caption('DataLabeler by Dan Denton and Jesse Selover (%d)' % current_image_index)
                elif event.key == K_a:
                    current_image_index = (current_image_index - 1) % NUM_IMAGES
                    background = LoadAerial(current_image_index, site_array)
                    allsprites = pygame.sprite.RenderPlain((background))
                    pygame.display.set_caption('DataLabeler by Dan Denton and Jesse Selover (%d)' % current_image_index)
           
            
            elif event.type == MOUSEBUTTONDOWN:
                site_coords = ScreenCoordsToSiteCoords(event.pos)
                print site_coords
                '''
                Mouse controls are:
                    left-click to label site as a 1 (blue tint)
                    shift-left click to label site as a 2 (red tint)
                    right click to label site as a 0
                '''
                if event.button == LEFT_BUTTON:
                    if (pygame.key.get_mods() & KMOD_LSHIFT):
                        site_array[site_coords[0]][site_coords[1]] = 2
                        SaveAerial(current_image_index, site_array)
                    else:    
                        site_array[site_coords[0]][site_coords[1]] = 1
                        SaveAerial(current_image_index, site_array)
                elif event.button == RIGHT_BUTTON:
                    site_array[site_coords[0]][site_coords[1]] = 0
                    SaveAerial(current_image_index, site_array)
            #elif event.type == MOUSEBUTTONUP:
            
        
        
        ''' Drawing update
        '''
        allsprites.draw(screen)
        screen.blit(grid, (0, 0))
        for i in range(site_array_dim):
            for j in range(site_array_dim):
                if site_array[i][j] == 1:
                    screen.blit(box1, SiteCoordsToScreenCoords((i, j)))
                elif site_array[i][j] == 2:
                    screen.blit(box2, SiteCoordsToScreenCoords((i, j)))
        pygame.display.flip()
            



if __name__ == '__main__':
    main(0)
        
    

            
    