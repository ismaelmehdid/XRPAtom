#!/bin/bash

DATA_URL="https://zenodo.org/records/7362094/files/imp-post.tzst?download=1"
METADATA_URL="https://zenodo.org/records/7362094/files/metadata.csv?download=1"

sudo apt update
sudo apt install zstd

TARGET_DIR="data"
mkdir -p "$TARGET_DIR"

if [ -f "$TARGET_DIR/imp-post.tzst" ]; then
    echo "Directory '$TARGET_DIR' already exists."
else
    wget -O "$TARGET_DIR/imp-post.tzst" "$DATA_URL"
    mkdir -p "$TARGET_DIR/imp-post"
    tar --use-compress-program=unzstd -xvf "$TARGET_DIR/imp-post.tzst" -C "$TARGET_DIR/imp-post"
fi

if [ -f "$TARGET_DIR/metadata.csv" ]; then
    echo "File 'metadata.csv' already exists in '$TARGET_DIR'."
else
    wget -O "$TARGET_DIR/metadata.csv" "$METADATA_URL"
fi
echo "Download complete. File stored in '$TARGET_DIR'"
