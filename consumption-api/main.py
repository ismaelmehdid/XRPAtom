from fastapi import FastAPI, HTTPException, Query
from datetime import datetime, timedelta
import random
import uuid

app = FastAPI()

ADDRESSES = [
    "123 Energy Street, Powertown, 90210",
    "456 Solar Ave, Bright City, 80001",
    "789 Hydro Road, Waterland, 75002"
]

def generate_mock_data(start_date: str, end_date: str, granularity: str):
    try:
        start = datetime.fromisoformat(start_date.replace("Z", ""))
        end = datetime.fromisoformat(end_date.replace("Z", ""))
    except ValueError:
        raise HTTPException(status_code=400, detail="Invalid date format. Use ISO 8601 (YYYY-MM-DDTHH:MM:SSZ)")

    if start > end:
        raise HTTPException(status_code=400, detail="start_date cannot be later than end_date")

    delta_mapping = {
        "hourly": timedelta(hours=1),
        "daily": timedelta(days=1),
        "monthly": timedelta(days=30)
    }
    
    if granularity not in delta_mapping:
        raise HTTPException(status_code=400, detail="Invalid granularity. Choose hourly, daily, or monthly.")

    delta = delta_mapping[granularity]
    readings = []
    total = 0
    peak = {"date": "", "value": 0}
    lowest = {"date": "", "value": float("inf")}

    current = start
    while current <= end:
        value = round(random.uniform(20, 40), 2)
        readings.append({"date": current.strftime("%Y-%m-%dT%H:%M:%SZ"), "value": value})
        total += value

        if value > peak["value"]:
            peak = {"date": current.strftime("%Y-%m-%dT%H:%M:%SZ"), "value": value}
        if value < lowest["value"]:
            lowest = {"date": current.strftime("%Y-%m-%dT%H:%M:%SZ"), "value": value}

        current += delta

    average = round(total / len(readings), 2) if readings else 0

    return {
        "unit": "kWh",
        "granularity": granularity,
        "period": {"start": start_date, "end": end_date},
        "readings": readings,
        "total": round(total, 2),
        "average": average,
        "peak": peak,
        "lowest": lowest
    }

@app.get("/consumption")
def get_consumption(
    pdl_id: str = Query(..., description="Point de Livraison ID"),
    start_date: str = Query(..., description="Start date in ISO 8601 (YYYY-MM-DDTHH:MM:SSZ)"),
    end_date: str = Query(..., description="End date in ISO 8601 (YYYY-MM-DDTHH:MM:SSZ)"),
    granularity: str = Query("daily", description="Granularity: hourly, daily, monthly")
):  
    mock_data = generate_mock_data(start_date, end_date, granularity)

    return {
        "meta": {
            "requestId": str(uuid.uuid4()),
            "timestamp": datetime.utcnow().isoformat() + "Z"
        },
        "pdl": {
            "id": pdl_id,
            "address": random.choice(ADDRESSES)
        },
        "consumption": mock_data
    }


# http://localhost:51244/consumption?pdl_id=555666777&start_date=2024-01-01T00:00:00Z&end_date=2025-01-01T00:00:00Z&granularity=monthly
# http://localhost:51244/consumption?pdl_id=987654321&start_date=2025-03-10T00:00:00Z&end_date=2025-03-15T00:00:00Z&granularity=daily
# http://localhost:51244/consumption?pdl_id=123456789&start_date=2025-03-15T00:00:00Z&end_date=2025-03-15T06:00:00Z&granularity=hourly


# without start date and end date