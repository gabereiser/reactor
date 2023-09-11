//go:build !windows

package assets

import "embed"

//go:generate ./_compile_shaders.sh
//go:embed shaders/*
var FS embed.FS
