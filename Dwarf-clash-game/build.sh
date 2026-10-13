#! /bin/bash

# параметры 
# ${1} - данные активированной лицензии Unity3D
# ${2} - путь к директории для результатов сборки под WebGL

set -e

UNITY3D_VERSION="$(cat /opt/unity/version)"
UNITY3D_EDITOR_PATH="/opt/unity/editors/${UNITY3D_VERSION}/Editor"
UNITY3D_LIC_DEC="$(echo ${1} | base64 -d)"
mkdir -p ~/.local/share/unity3d/Unity "${2}"
sed -i "s|%%%WebGLDwarfClashBuild%%%|$2|g" "${CI_PROJECT_DIR}/Dwarf-clash-game/Assets/Editor/GameBuilder.cs"
echo "${UNITY3D_LIC_DEC}" > ~/.local/share/unity3d/Unity/Unity_lic.ulf
xvfb-run -a "${UNITY3D_EDITOR_PATH}/Unity" -quit -batchmode -projectPath "${CI_PROJECT_DIR}/Dwarf-clash-game" -executeMethod GameBuilder.PerformBuild -logFile /tmp/GameBuild.log && UNITY3D_RETVAL=$? || UNITY3D_RETVAL=$?
if [ "${UNITY3D_RETVAL}" != "0" ]; then
    tail -1000 /tmp/GameBuild.log
    exit 1
fi
