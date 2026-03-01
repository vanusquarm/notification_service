SELECT tran_date, narration, amount, balance
FROM account_transactions
WHERE account_no = :acc
  AND tran_date BETWEEN :fromDate AND :toDate
ORDER BY tran_date