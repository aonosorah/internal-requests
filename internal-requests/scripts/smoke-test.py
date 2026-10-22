
import json
import urllib.request
import urllib.error

from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from uuid import uuid4


BASE_URL = "http://localhost:5000/api/demands"


def request(method, path="", payload=None):
    data = json.dumps(payload).encode() if payload is not None else None

    headers = {
        "Content-Type": "application/json"
    }

    req = urllib.request.Request(
        BASE_URL + path,
        data=data,
        headers=headers,
        method=method
    )

    try:
        with urllib.request.urlopen(req, timeout=15) as response:
            body = response.read().decode()

            return (
                response.status,
                json.loads(body) if body else None
            )

    except urllib.error.HTTPError as error:
        return error.code, error.read().decode()

    except Exception as error:
        return 0, str(error)


def demand(description="Solicitação comum", **changes):
    payload = {
        "title": f"Teste {uuid4().hex[:8]}",
        "description": description,
        "sector": "Qualidade",
        "requestDate": datetime.now(timezone.utc).isoformat(),
        "status": "Pending"
    }

    payload.update(changes)
    return payload


def check(name, condition, detail=""):
    result = "PASSOU" if condition else "FALHOU"

    print(f"[{result}] {name} {detail}")

    return condition


def normalize_date(value):
    """
    Normaliza datas para UTC antes da comparação.

    Neste teste, datas sem fuso são interpretadas
    como UTC, pois o cadastro envia datas em UTC.
    """
    if not isinstance(value, str):
        return None

    try:
        parsed = datetime.fromisoformat(
            value.replace("Z", "+00:00")
        )

        if parsed.tzinfo is None:
            parsed = parsed.replace(tzinfo=timezone.utc)

        return parsed.astimezone(timezone.utc)

    except ValueError:
        return None


def diagnose_preserved_fields(original, updated):
    """
    Compara os dados antes e depois do PATCH.

    Datas são comparadas como instantes UTC,
    e os demais campos são comparados diretamente.
    """
    fields = (
        "id",
        "title",
        "description",
        "sector",
        "requestDate",
        "priority"
    )

    print("\nDiagnóstico dos campos preservados:")

    if not isinstance(original, dict):
        print("Resposta original não é um objeto JSON.")
        return False

    if not isinstance(updated, dict):
        print("Resposta do PATCH não é um objeto JSON.")
        print(f"Resposta recebida: {updated!r}")
        return False

    all_preserved = True

    for field in fields:
        before = original.get(field)
        after = updated.get(field)

        if field == "requestDate":
            before_date = normalize_date(before)
            after_date = normalize_date(after)

            preserved = (
                before_date is not None
                and after_date is not None
                and before_date == after_date
            )
        else:
            preserved = (
                field in original
                and field in updated
                and before == after
            )

        result = "OK" if preserved else "DIFERENTE"

        print(f"  {field}: {result}")

        if not preserved:
            all_preserved = False

            print(f"    Antes:  {before!r}")
            print(f"    Depois: {after!r}")

    if original.get("priority") != "High":
        all_preserved = False

        print(
            "  [ALERTA] A prioridade original deveria ser High."
        )

    return all_preserved


results = []


# 1. GET - Listagem de demandas

status, data = request("GET")

results.append(check(
    "GET listagem",
    status == 200 and isinstance(data, list),
    f"HTTP {status}"
))


# 2. POST - Prioridade alta

status, data = request(
    "POST",
    payload=demand("Erro URGENTE no sistema")
)

results.append(check(
    "Prioridade alta",
    status == 201
    and isinstance(data, dict)
    and data.get("priority") == "High",
    f"HTTP {status}"
))


# 3. POST - Prioridade normal

status, data = request(
    "POST",
    payload=demand("Solicitação de teclado")
)

results.append(check(
    "Prioridade normal",
    status == 201
    and isinstance(data, dict)
    and data.get("priority") == "Normal",
    f"HTTP {status}"
))


# 4-6. POST - Validação dos campos obrigatórios

for field in ("title", "description", "sector"):
    status, _ = request(
        "POST",
        payload=demand(**{field: ""})
    )

    results.append(check(
        f"Campo obrigatório: {field}",
        status == 400,
        f"HTTP {status}"
    ))


# 7. POST - Status inválido

status, _ = request(
    "POST",
    payload=demand(status="InvalidStatus")
)

results.append(check(
    "Status inválido no cadastro",
    status == 400,
    f"HTTP {status}"
))


# 8. GET - ID inexistente

status, _ = request(
    "GET",
    f"/{uuid4()}"
)

results.append(check(
    "ID inexistente na consulta",
    status == 404,
    f"HTTP {status}"
))


# 9. POST - Teste de concorrência

print("\nTeste de concorrência: 20 cadastros simultâneos")


def create_one(_):
    return request(
        "POST",
        payload=demand("Teste de concorrência")
    )


with ThreadPoolExecutor(max_workers=5) as executor:
    responses = list(
        executor.map(create_one, range(20))
    )


created = [
    body
    for code, body in responses
    if code == 201 and isinstance(body, dict)
]

ids = [
    body.get("id")
    for body in created
    if body.get("id")
]

results.append(check(
    "20 cadastros simultâneos",
    len(created) == 20
    and len(ids) == 20
    and len(set(ids)) == 20,
    f"{len(created)}/20 criados"
))


# 10-15. PATCH - Testes de integração

print("\nTestes de integração: atualização de status")

status, original = request(
    "POST",
    payload=demand("Erro urgente no processamento")
)

if (
    status == 201
    and isinstance(original, dict)
    and original.get("id")
):
    demand_id = original["id"]

    # 10. Pending -> InProgress

    patch_status, updated = request(
        "PATCH",
        f"/{demand_id}/status",
        {"status": "InProgress"}
    )

    results.append(check(
        "PATCH para Em andamento",
        patch_status == 200
        and isinstance(updated, dict)
        and updated.get("status") == "InProgress",
        f"HTTP {patch_status}"
    ))

    # 11. Persistência após GET

    get_status, persisted = request(
        "GET",
        f"/{demand_id}"
    )

    results.append(check(
        "Persistência do status após GET",
        get_status == 200
        and isinstance(persisted, dict)
        and persisted.get("status") == "InProgress",
        f"HTTP {get_status}"
    ))

    # 12. Preservação dos dados

    fields_preserved = diagnose_preserved_fields(
        original,
        updated
    )

    results.append(check(
        "Dados e prioridade preservados",
        fields_preserved
    ))

    # 13. InProgress -> Completed

    patch_status, completed = request(
        "PATCH",
        f"/{demand_id}/status",
        {"status": "Completed"}
    )

    results.append(check(
        "PATCH para Concluída",
        patch_status == 200
        and isinstance(completed, dict)
        and completed.get("status") == "Completed",
        f"HTTP {patch_status}"
    ))

    # 14. PATCH com status inválido

    invalid_status, _ = request(
        "PATCH",
        f"/{demand_id}/status",
        {"status": "InvalidStatus"}
    )

    results.append(check(
        "PATCH com status inválido",
        invalid_status == 400,
        f"HTTP {invalid_status}"
    ))

    # 15. Garantir que o status não mudou

    get_status, unchanged = request(
        "GET",
        f"/{demand_id}"
    )

    results.append(check(
        "Status preservado após PATCH inválido",
        get_status == 200
        and isinstance(unchanged, dict)
        and unchanged.get("status") == "Completed",
        f"HTTP {get_status}"
    ))

else:
    print(
        "[FALHOU] Não foi possível criar a demanda "
        "para testar o PATCH."
    )

    results.extend([False] * 6)


# 16. PATCH - ID inexistente

missing_status, _ = request(
    "PATCH",
    f"/{uuid4()}/status",
    {"status": "Completed"}
)

results.append(check(
    "PATCH com ID inexistente",
    missing_status == 404,
    f"HTTP {missing_status}"
))


# Resultado final

passed = sum(results)
total = len(results)

print(f"\nResultado: {passed}/{total} verificações passaram")

if not all(results):
    raise SystemExit(1)
