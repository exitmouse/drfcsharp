from PIL import Image
import glob, os, random

x_size = 256
y_size = 256

for infile in glob.glob("../*.jp*g"):
    rotation = random.randint(0,3)
    filename, ext = os.path.splitext(infile)
    filenum = filename[3:]
    im = Image.open(infile)
    im = im.rotate(rotation*90)
    width, height = im.size
    x_offset = random.randint(0, width-x_size)
    y_offset = random.randint(0, height-y_size)
    im = im.crop((x_offset,y_offset, x_offset+x_size, y_offset+y_size))
    im.save("../RandCropRotate"+filenum+".jpg","JPEG")
