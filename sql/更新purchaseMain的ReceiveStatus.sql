UPDATE 
    purchase_main_sheet p
SET 
    p.ReceiveStatus = 'ALL_ACCEPT'
WHERE 
    p.ReceiveStatus != 'ALL_ACCEPT'
    AND NOT EXISTS (
        SELECT 1
        FROM acceptance_item a
        WHERE 
            a.PurchaseMainId = p.PurchaseMainId
            AND a.InStockStatus != 'DONE'
    );
