#!/bin/bash

# Script para verificar que todos los PageModels están correctamente creados

echo "=========================================="
echo "  VERIFICACIÓN DE PAGEMODELS"
echo "=========================================="
echo ""

PAGES_DIR="Pages"
ERRORS=0

echo "1. Verificando que cada .cshtml tiene su .cshtml.cs..."
echo ""

for cshtml in $(find $PAGES_DIR -name "*.cshtml" -not -name "_*.cshtml" -not -path "*/Shared/*"); do
    cshtml_cs="${cshtml}.cs"
    basename=$(basename "$cshtml")
    dirname=$(dirname "$cshtml")
    
    if [ -f "$cshtml_cs" ]; then
        echo "   ✅ $dirname/$basename → OK"
    else
        echo "   ❌ $dirname/$basename → FALTA $basename.cs"
        ERRORS=$((ERRORS + 1))
    fi
done

echo ""
echo "2. Verificando estructura de namespaces..."
echo ""

for cshtml_cs in $(find $PAGES_DIR -name "*.cshtml.cs" -not -path "*/Shared/*"); do
    # Extraer el área del path
    if [[ "$cshtml_cs" =~ Pages/([^/]+)/ ]]; then
        area="${BASH_REMATCH[1]}"
        
        # Verificar que el namespace sea correcto
        expected_ns="namespace TiendaDawWeb.Web.RazorPages.Pages.$area;"
        
        if grep -q "$expected_ns" "$cshtml_cs"; then
            echo "   ✅ $(basename $cshtml_cs) → Namespace correcto"
        else
            echo "   ⚠️  $(basename $cshtml_cs) → Revisar namespace"
        fi
    fi
done

echo ""
echo "3. Resumen de archivos..."
echo ""

total_cshtml=$(find $PAGES_DIR -name "*.cshtml" -not -name "_*.cshtml" -not -path "*/Shared/*" | wc -l)
total_pagemodels=$(find $PAGES_DIR -name "*.cshtml.cs" -not -path "*/Shared/*" | wc -l)

echo "   Total .cshtml:     $total_cshtml"
echo "   Total .cshtml.cs:  $total_pagemodels"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo "=========================================="
    echo "  ✅ VERIFICACIÓN COMPLETADA"
    echo "  No se encontraron errores"
    echo "=========================================="
    exit 0
else
    echo "=========================================="
    echo "  ⚠️  SE ENCONTRARON $ERRORS ERRORES"
    echo "  Revisa los archivos marcados arriba"
    echo "=========================================="
    exit 1
fi
