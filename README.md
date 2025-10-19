# Un CRUD sobre la IndexedDB 
La aplicación realiza un **CRUD** sobre la base de datos local **IndexedDB**.
Para aquellos que no conocen lo que es un:

**CRUD:** Crear (Create), Leer (Read), Actualizar (Update) y Eliminar (Delete)

**IndexedDB:** base de datos local NoSQL

La aplicación es **web**, se ha utilizado lenguaje **C#** + **Blazor WebAssembly (WASM)**

[Publicacion de la aplicación](https://github.com/HoracioAldoTore/Blazor_IndexedDB_CRUD)

## Pasos a seguir para la instalación que facilita el CRUD:

**1)** Instalar el paquete de NuGet de Blazor.IndexedDB version 3.0.3. Siguindo lo indicado en: https://www.nuget.org/packages/Blazor.IndexedDB

**2)** Copiar el archivo **BlazorIndexedDBExtencion.cs** en su proyecto para acceder a los metodos de extensión. Los métodos de extensión se encuentran definidos en el archivo BlazorIndexedDBExtencion.cs y utilizan reflection c# para descubrir métodos y propiedades.

**3)** Crear el modelo de base heredando de IndexedDb, puede tomar como ejemplo el arrchivo ExampleDb.cs

## Otro codigo relacionado al desarrollo:
[Blazor IndexedDB Extencion](https://github.com/HoracioAldoTore/BlazorIndexedDBExtencion)
