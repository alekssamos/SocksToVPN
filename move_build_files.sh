#!/bin/bash

# Исходная папка
base_dir="publish"
# Целевая папка, где будут собраны все файлы
target_folder="combined_files"

# Создаем целевую папку
mkdir -p "$target_folder"

# Проходим по всем подпапкам в base_dir
for dir in "$base_dir"/*; do
    if [[ -d "$dir" ]]; then  # Проверяем, что это папка
        platform=$(basename "$dir")  # Извлекаем название платформы (например, linux-arm64)

        # Перемещаем файлы из текущей подпапки
        for file in "$dir"/*; do
            if [[ -f "$file" ]]; then  # Проверяем, что это файл
                # Получаем имя файла без расширения
                filename=$(basename "$file")
                # Проверяем, если файл не .pdb
                if [[ "$filename" != *.pdb ]]; then
                    # Переименовываем файл в формате platform_platform_name
                    mv "$file" "$target_folder/${platform}_${filename}"
                fi
            fi
        done
        # Удаляем .pdb файлы в текущей подпапке
        rm -f "$dir"/*.pdb
    fi
done

echo "Все файлы перемещены в $target_folder. .pdb файлы удалены."
