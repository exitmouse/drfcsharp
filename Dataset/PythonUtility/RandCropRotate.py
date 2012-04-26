from PIL import Image
import glob, os, random

x_size = 256
y_size = 256

for infile in glob.glob("*.jp*g"):
    rotation = random.randint(0,3)
    file, ext = os.path.splitext(infile)
    im = Image.open(infile)
    im = im.rotate(rotation*90)
    width, height = im.size()
    x_offset = random.randint(0, width-x_size)
    y_offset = random.randint(0, height-y_size)
    im.crop(x_offset,y_offset + y_size, x_offset+x_size, y_offset)
    im.save(file + "_rand_crop_rotate.jpg","JPEG")
