---------------------------------------------------------------------------------------------------------
 Soluci�n de algunos de errores en MONODEVELOP
---------------------------------------------------------------------------------------------------------

-Error: Al compilar nos dice: No se permiten especificadores de par�metros predeterminados.
-Soluci�n: Project > Assembly-CSharp options > Build > General > Target Framework > Mono / .NET 4.0

---------------------------------------------------------------------------------------------------------

-Error: Al compilar nos dice: El c�digo no seguro s�lo puede aparecer si se compila con /unsafe
-Soluci�n: Project > Assembly-CSharp options > Build > General > Allow 'unsafe' code