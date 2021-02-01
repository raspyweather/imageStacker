# ImageStacker CLI Documentation

## Input 
### Input from video
    --inputFile

### Input from multiple images
Directory with Files:
    --inputFolder 
    --inputFilter *.JPG

List of Files:
    --inputFiles a,b,c,d

### Input from Pipe
Very very beta :)
    --inputByPipe
    --inputSize 1920x1080

## Output

### Output to video
    --outputFile a.mp4
    --outputVideoOptions "..."
    --outputPreset FHD, 4K 

### Output to Pipe
Very very beta :)
    --outputToPipe

### Output to images
    --outputFolder 
    --outputFilePrefix
    --outputFileType JPG/BMP/PNG/TIFF

## Processing

### Modes
#### stackAll
Applies filters to all images, resulting in a single one.

    src1,src2,src3,src4,src5 → result_1

#### stackContinuous
Applies filters to the last k images, resulting in ```n-k``` images.

    for n=3
    src1,src2,src3,src4,src5 → result_1 (1,2,3),result_2 (2,3,4),result_3 (3,4,5)

#### stackProgressive
Applies filters to each image including their predecessors.

    src1,src2,src3 → result_1 (1),result_2 (1,2),result_3 (1,2,3)

### Filters

#### MaxFilter
Selects maximum values per pixel per channel. Useful for stars, lighttrails etc.

#### MinFilter
Selects minimum values per pixel per channel. Useful for birds, planes etc.

#### AttackDecayFilter
Applies an AttackDecayFilter to each channel per pixel.

##### Parameters

- *Attack* defines how fast the image will brighten up when something bright occurs. Setting it to a lower values soften extrema.
- *Decay* defines how fast the image darkens back down. Higher values result in faster fading. For moving light sources effectively defining the length of the trail

Useful values: 
- ```Attack=1, Decay=0.2```  immediate brightening, quick fading, useful for thunderstorms
- ```Attack=1, Decay=0.05``` immediate brightening, slow fading, useful for rather longer lighttrails etc.
- ```Attack=1, Decay=0```  immediate brightening, no fading, equivalent to MaxFilter

- ```Attack=0.2, Decay=1```  quick brightening, immediate fading, useful for birds, helicopters
- ```Attack=0.05, Decay=1``` slow brightening, immediate fading, useful for birds, helicopters etc.
- ```Attack=0, Decay=1```  no brightening, immediate fading, equivalent to MinFilter

### Bitdepth
Currently only images with 8 bit per channel are supported.