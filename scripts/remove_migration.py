#!/usr/bin/env python3
"""
Entity Framework Core migration remove script.

Removes the last migration from the database context.

Usage:
    python remove_migration.py
"""

import sys
import subprocess
import os
from pathlib import Path

def main():
    # Get the workspace root (parent of scripts folder)
    script_dir = Path(__file__).parent.absolute()
    workspace_root = script_dir.parent
    
    # Project path relative to workspace root
    project_path = workspace_root / "src" / "Server" / "Server.csproj"
    
    # Verify project file exists
    if not project_path.exists():
        print(f"Error: Project file not found at {project_path}")
        sys.exit(1)
    
    # Build the dotnet ef command
    # Use relative path from workspace root for cleaner output
    project_relative = project_path.relative_to(workspace_root)
    command = [
        "dotnet", "ef", "migrations", "remove",
        "--project", str(project_relative),
        "--startup-project", str(project_relative),
        "--context", "ApplicationDbContext"
    ]
    
    print("Removing last migration...")
    print(f"Project: {project_relative}")
    print(f"Context: ApplicationDbContext")
    print()
    
    # Change to workspace root directory
    os.chdir(workspace_root)
    
    # Check if dotnet-ef tool is installed
    try:
        check_result = subprocess.run(
            ["dotnet", "ef", "--version"],
            capture_output=True,
            text=True,
            timeout=5
        )
        if check_result.returncode != 0:
            raise subprocess.CalledProcessError(check_result.returncode, ["dotnet", "ef", "--version"])
    except (subprocess.CalledProcessError, FileNotFoundError, subprocess.TimeoutExpired):
        print()
        print("✗ Error: Entity Framework Core tools (dotnet-ef) not found.")
        print()
        print("To install the EF Core tools, run:")
        print("  dotnet tool install --global dotnet-ef")
        print()
        print("Or if you prefer a local tool, run:")
        print("  dotnet tool install dotnet-ef")
        sys.exit(1)
    
    # Execute the command
    try:
        result = subprocess.run(
            command,
            check=True,
            capture_output=False,  # Show output in real-time
            text=True
        )
        print()
        print("✓ Last migration removed successfully!")
        sys.exit(0)
    except subprocess.CalledProcessError as e:
        print()
        print(f"✗ Error removing migration: {e}")
        sys.exit(1)
    except FileNotFoundError:
        print()
        print("✗ Error: 'dotnet' command not found. Make sure .NET SDK is installed and in your PATH.")
        sys.exit(1)

if __name__ == "__main__":
    main()
