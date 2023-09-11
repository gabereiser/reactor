package opengl

import (
	gl "github.com/go-gl/gl/v4.1-core/gl"
)

func Init() {
	if err := gl.Init(); err != nil {
		panic(err)
	}
}
