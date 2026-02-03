#!/usr/bin/env python3
"""
Script para generar PageModels automáticamente desde controladores MVC
"""
import os
import re
from pathlib import Path
from typing import Dict, List, Optional, Tuple

# Configuración
PROJECT_ROOT = r"C:\Users\joseluisgs\Projects\Tienda\TiendaDawWeb-NetCore"
RAZOR_PAGES_ROOT = os.path.join(PROJECT_ROOT, "TiendaDawWeb.Web.RazorPages", "Pages")
CONTROLLERS_ROOT = os.path.join(PROJECT_ROOT, "TiendaDawWeb.Web", "Controllers")

# Mapeo de vistas a controladores
VIEW_MAPPING = {
    # Public
    "Public/Index.cshtml": {"controller": "PublicController", "action": "Index", "methods": ["GET"]},
    
    # Auth
    "Auth/Login.cshtml": {"controller": "AuthController", "action": "Login", "methods": ["GET", "POST"]},
    "Auth/Register.cshtml": {"controller": "AuthController", "action": "Register", "methods": ["GET", "POST"]},
    "Auth/AccessDenied.cshtml": {"controller": "AuthController", "action": "AccessDenied", "methods": ["GET"]},
    
    # Product
    "Product/Index.cshtml": {"controller": "ProductController", "action": "Index", "methods": ["GET"]},
    "Product/Details.cshtml": {"controller": "ProductController", "action": "Details", "methods": ["GET"]},
    "Product/Create.cshtml": {"controller": "ProductController", "action": "Create", "methods": ["GET", "POST"]},
    "Product/Edit.cshtml": {"controller": "ProductController", "action": "Edit", "methods": ["GET", "POST"]},
    "Product/MyProducts.cshtml": {"controller": "ProductController", "action": "MyProducts", "methods": ["GET"]},
    
    # Admin
    "Admin/Index.cshtml": {"controller": "AdminController", "action": "Index", "methods": ["GET"]},
    "Admin/Usuarios.cshtml": {"controller": "AdminController", "action": "Usuarios", "methods": ["GET"]},
    "Admin/UsuarioDetails.cshtml": {"controller": "AdminController", "action": "UsuarioDetails", "methods": ["GET"]},
    "Admin/Productos.cshtml": {"controller": "AdminController", "action": "Productos", "methods": ["GET"]},
    "Admin/Compras.cshtml": {"controller": "AdminController", "action": "Compras", "methods": ["GET"]},
    "Admin/Ventas.cshtml": {"controller": "AdminController", "action": "Ventas", "methods": ["GET"]},
    "Admin/Estadisticas.cshtml": {"controller": "AdminController", "action": "Estadisticas", "methods": ["GET"]},
    
    # Carrito
    "Carrito/Index.cshtml": {"controller": "CarritoController", "action": "Index", "methods": ["GET"]},
    "Carrito/Resumen.cshtml": {"controller": "CarritoController", "action": "Resumen", "methods": ["GET"]},
    
    # Purchase
    "Purchase/Index.cshtml": {"controller": "PurchaseController", "action": "Index", "methods": ["GET"]},
    "Purchase/Details.cshtml": {"controller": "PurchaseController", "action": "Details", "methods": ["GET"]},
    "Purchase/Confirmacion.cshtml": {"controller": "PurchaseController", "action": "Confirmacion", "methods": ["GET"]},
    
    # Profile
    "Profile/Index.cshtml": {"controller": "ProfileController", "action": "Index", "methods": ["GET"]},
    "Profile/Edit.cshtml": {"controller": "ProfileController", "action": "Edit", "methods": ["GET", "POST"]},
    "Profile/ChangePassword.cshtml": {"controller": "ProfileController", "action": "ChangePassword", "methods": ["GET", "POST"]},
    
    # Favorite
    "Favorite/Index.cshtml": {"controller": "FavoriteController", "action": "Index", "methods": ["GET"]},
}


def read_controller(controller_path: str) -> Optional[str]:
    """Lee el contenido de un controlador"""
    if not os.path.exists(controller_path):
        return None
    with open(controller_path, 'r', encoding='utf-8') as f:
        return f.read()


def extract_usings(controller_content: str) -> List[str]:
    """Extrae los usings necesarios del controlador"""
    base_usings = [
        "using Microsoft.AspNetCore.Mvc;",
        "using Microsoft.AspNetCore.Mvc.RazorPages;",
        "using Microsoft.AspNetCore.Identity;",
        "using TiendaDawWeb.Shared.Models;",
    ]
    
    additional_patterns = [
        (r"using ([^;]+Services[^;]+);", lambda m: m.group(0)),
        (r"using TiendaDawWeb\.Shared\.Data;", "using TiendaDawWeb.Shared.Data;"),
        (r"using TiendaDawWeb\.Shared\.ViewModels;", "using TiendaDawWeb.Shared.ViewModels;"),
        (r"using TiendaDawWeb\.Shared\.Mappers;", "using TiendaDawWeb.Shared.Mappers;"),
        (r"using TiendaDawWeb\.Shared\.Models\.Enums;", "using TiendaDawWeb.Shared.Models.Enums;"),
        (r"using Microsoft\.EntityFrameworkCore;", "using Microsoft.EntityFrameworkCore;"),
        (r"using Microsoft\.AspNetCore\.SignalR;", "using Microsoft.AspNetCore.SignalR;\nusing TiendaDawWeb.Shared.Web.Hubs;"),
        (r"using Microsoft\.AspNetCore\.Localization;", "using Microsoft.AspNetCore.Localization;"),
        (r"using Microsoft\.AspNetCore\.OutputCaching;", "using Microsoft.AspNetCore.OutputCaching;"),
    ]
    
    usings = base_usings.copy()
    for pattern, replacement in additional_patterns:
        match = re.search(pattern, controller_content)
        if match:
            if callable(replacement):
                usings.append(replacement(match))
            else:
                for line in replacement.split('\n'):
                    if line.strip():
                        usings.append(line)
    
    return sorted(set(usings))


def extract_constructor_params(controller_content: str, controller_name: str) -> str:
    """Extrae los parámetros del constructor"""
    match = re.search(rf"public class {controller_name}\(([^)]+)\)", controller_content)
    return match.group(1).strip() if match else ""


def extract_authorize_attribute(controller_content: str) -> str:
    """Extrae el atributo de autorización"""
    if re.search(r'\[Authorize\(Roles = "([^"]+)"\)\]', controller_content):
        match = re.search(r'\[Authorize\(Roles = "([^"]+)"\)\]', controller_content)
        return f'[Microsoft.AspNetCore.Authorization.Authorize(Roles = "{match.group(1)}")]\n'
    elif re.search(r'\[Authorize\]', controller_content):
        return "[Microsoft.AspNetCore.Authorization.Authorize]\n"
    return ""


def generate_pagemodel(
    area: str,
    page_name: str,
    controller_name: str,
    action_name: str,
    methods: List[str],
    controller_content: str
) -> str:
    """Genera el código del PageModel"""
    
    namespace = f"TiendaDawWeb.Web.RazorPages.Pages.{area}"
    class_name = f"{page_name}Model"
    
    # Extraer información del controlador
    usings = extract_usings(controller_content)
    constructor_params = extract_constructor_params(controller_content, controller_name)
    authorize = extract_authorize_attribute(controller_content)
    
    # Generar código
    code_parts = []
    code_parts.append("\n".join(usings))
    code_parts.append(f"\nnamespace {namespace};\n")
    if authorize:
        code_parts.append(authorize)
    
    code_parts.append(f"public class {class_name}({constructor_params}) : PageModel {{")
    
    # Determinar si hay POST y extraer ViewModel
    has_post = "POST" in methods
    bind_property = ""
    
    if has_post:
        # Buscar el tipo del ViewModel en el método POST
        post_match = re.search(rf"Task<IActionResult> {action_name}\([^)]*?(\w+ViewModel)[^)]*\)", controller_content)
        if post_match:
            viewmodel_type = post_match.group(1)
            bind_property = f"""    [BindProperty]
    public {viewmodel_type} Input {{ get; set; }} = default!;

"""
            code_parts.append(bind_property)
    
    # Método OnGet
    code_parts.append(generate_onget_method(controller_content, action_name))
    
    # Método OnPost
    if has_post:
        code_parts.append(generate_onpost_method(controller_content, action_name, page_name))
    
    code_parts.append("}\n")
    
    return "\n".join(code_parts)


def generate_onget_method(controller_content: str, action_name: str) -> str:
    """Genera el método OnGet"""
    # Buscar método GET (puede tener o no [HttpGet])
    get_patterns = [
        rf"(?s)\[HttpGet\][^\n]*\s*public (async )?Task<IActionResult> {action_name}\(([^)]*)\)\s*\{{([^}}]*(?:\{{[^}}]+\}}[^
