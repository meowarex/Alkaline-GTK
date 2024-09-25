from setuptools import setup, find_packages

setup(
    name="Alkaline",
    version="0.1",
    packages=find_packages(where="src"),
    package_dir={"": "src"},
    install_requires=[
        "PyGObject",
        "requests",
    ],
    entry_points={
        "console_scripts": [
            "alkaline=alkaline_app:main",
        ],
    },
    data_files=[
        ('share/applications', ['data/com.atomix.Alkaline.desktop']),
        ('share/icons/hicolor/48x48/apps', ['assets/icons/alkaline.png']),
    ],
)