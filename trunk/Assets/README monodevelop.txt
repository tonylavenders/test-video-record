---------------------------------------------------------------------------------------------------------
 Solución de algunos de errores en MONODEVELOP
---------------------------------------------------------------------------------------------------------

-Error: Al compilar nos dice: No se permiten especificadores de parámetros predeterminados.
-Solución: Project > Assembly-CSharp options > Build > General > Target Framework > Mono / .NET 4.0

---------------------------------------------------------------------------------------------------------

-Error: Al compilar nos dice: El código no seguro sólo puede aparecer si se compila con /unsafe
-Solución: Project > Assembly-CSharp options > Build > General > Allow 'unsafe' code