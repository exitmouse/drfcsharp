'''
Created on May 27, 2012

@author: Dan Denton and Jesse Selover


This is a script to convert Kumar and Hebert's data labelings (which are stored as
tiff images) to our format as comma separated text files.
'''

import os, sys
import numpy
import Image
import csv
import glob
import random


CONVERTER_DIR = os.path.split(os.path.abspath(sys.argv[0]))[0]
DATASET_DIR = os.path.normpath(os.path.join(CONVERTER_DIR, '../../Dataset'))
DATASETKH_DIR = os.path.normpath(os.path.join(CONVERTER_DIR, '../../DatasetKH'))
DATASETKH_IMAGES_TEST_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'totalImagesTest/'))
DATASETKH_IMAGES_TRAIN_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'totalImagesTrain/'))
DATASETKH_LABELS_TEST_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'totalLabelsTest/'))
DATASETKH_LABELS_TRAIN_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'totalLabelsTrain/'))
DATASETKH_CSV_LABELS_TEST_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'csvLabelsTest/'))
DATASETKH_CSV_LABELS_TRAIN_DIR = os.path.normpath(os.path.join(DATASETKH_DIR, 'csvLabelsTrain/'))



def LoadKHLabeling(path):
    im = Image.open(path)
    return im


''' A method to save the labeling in the site array to a comma separated
text file named <image_index>.txt, located in the Dataset directory.
'''
def SaveLabeling(image_data_filename, site_array):
    
    print image_data_filename
    
    try:
        f = open(image_data_filename, "w")
        try:
            site_array_size = site_array.shape
            for y in range (site_array_size[0]):
                line = ''
                for x in range (site_array_size[1]):
                    line = line + ('%d,' % site_array[y,x])
                line = line + '\n'
                f.write(line)
        finally:
            f.close()
    except IOError:
        print "no open"
        pass
    
    
def generateSiteArray(isTestImage, imageNumber):
    current_data_index_str = str(imageNumber).zfill(3)
    if isTestImage:
        path1 = DATASETKH_LABELS_TEST_DIR + '/' + current_data_index_str + '.tif'
        path2 = DATASETKH_LABELS_TEST_DIR + '/s' + current_data_index_str + '.tif'
    else:
        path1 = DATASETKH_LABELS_TRAIN_DIR + '/' + current_data_index_str + '.tif'
        path2 = DATASETKH_LABELS_TRAIN_DIR + '/s' + current_data_index_str + '.tif'
        
    print path1
    print path2
    im1 = LoadKHLabeling(path1)
    im1pxl = im1.load()
    im2 = LoadKHLabeling(path2)
    im2pxl = im2.load()
    
    site_array = numpy.zeros(((im1.size)[1],(im1.size)[0]), dtype=numpy.int)
    site_array_size = site_array.shape
    for x in range (site_array_size[1]):
        for y in range (site_array_size[0]):
            if im1pxl[x,y] > 0 or im2pxl[x,y] > 0:
                site_array[y,x] = 1
                
    return site_array


if __name__ == '__main__':
    
    '''filenames = glob.glob(os.path.normpath(os.path.join(DATASETKH_LABELS_TEST_DIR, '*.tif')))
    m = (len(filenames) + 1) /2
    i = 0
    for filename in filenames:
        if i >= m:
            index_str = str(i-m).zfill(3)
            os.rename(filename, os.path.normpath(os.path.join(DATASETKH_LABELS_TEST_DIR, 's' + index_str + '.tif')))
        i += 1'''
    
    '''filenames = glob.glob(os.path.normpath(os.path.join(DATASETKH_LABELS_TEST_DIR, '*.tif')))
    for i in range(len(filenames)/2):
        site_array = generateSiteArray(True, i)
        
        current_data_index_str = str(i).zfill(3)
        current_data_name = current_data_index_str + '.txt'
        path = os.path.join(DATASETKH_CSV_LABELS_TEST_DIR, current_data_name)
        SaveLabeling(path, site_array)'''
   
    '''paths = glob.glob(os.path.normpath(os.path.join(DATASETKH_CSV_LABELS_TEST_DIR, '[0-9][0-9][0-9].txt'))) 
    for i in range(len(paths)):
        if i <= 20:
            new_path = os.path.normpath(os.path.join(DATASETKH_CSV_LABELS_TEST_DIR, str(i + 108).zfill(3) + '.txt'))
            print paths[i]
            print new_path
            os.system('git mv ' + paths[i] + ' ' + new_path) '''
    
    perm = range(300)
    random.shuffle(perm)
    print perm
    for i in range(300):
       path = os.path.normpath(os.path.join(DATASET_DIR, str(i).zfill(3) + '.txt'))
       new_path = os.path.normpath(os.path.join(DATASET_DIR, 'tmp/' + str(perm[i]).zfill(3) + '.txt'))
       #print path
       #print new_path
       cmd = 'git mv ' + path + ' ' + new_path
       print cmd
       os.system(cmd) 
       path = os.path.normpath(os.path.join(DATASET_DIR, str(i).zfill(3) + '.jpg'))
       new_path = os.path.normpath(os.path.join(DATASET_DIR, 'tmp/' + str(perm[i]).zfill(3) + '.jpg'))
       #print path
       #print new_path
       cmd = 'git mv ' + path + ' ' + new_path
       print cmd
       os.system(cmd) 
    
    #cmd = 'cd ' + DATASET_DIR + "; ls"
    #os.system(cmd)
       
    
    pass