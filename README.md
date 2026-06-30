# School Management System

A console application for managing a school, developed in C# with .NET 8.

## Features

### People Management
- Add students, teachers, and administrative staff
- Search for a person by ID
- List people by category

### Academic Management
- Create subjects (with coefficient and available seats)
- Schedule courses
- Enroll students in courses
- Assign grades
- Calculate weighted general average by coefficient

### Financial Management
- Calculate teacher salaries (base + overtime hours)
- Calculate administrative staff salaries (fixed + annual bonus)
- Generate payslips
- Calculate total payroll

### Administration
- Action history (Undo pattern with Stack)
- Save/Load data in JSON format
- Automatic data persistence

## Architecture

- **Interfaces**: `IPersonne`, `IPayable`
- **Abstract Classes**: `Personne`
- **Concrete Classes**: `Etudiant`, `Professeur`, `Administration`, `Matiere`, `Cours`
- **Generics**: `Repository&lt;T&gt;` for entity management
- **JSON Serialization**: automatic data saving

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 (recommended)

## Getting Started

```bash
dotnet run
