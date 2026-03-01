---FOR TRANSACTIONS BEFORE CUTOVER 14-10-2024 AND ABOVE (BASIS AND FINACLE TRANSACTIONS), 
---THIS FIRST QUERY WILL RUN (CHECK LAST CONDITION THERE)

--------PICK THE MINIMUM TRNSACTION DATE WITHIN THE SPOOL PERIOD
WITH MTD as (select min(t.tran_date) MIN_TRA_DATE
From CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
               tbaadm.sol                   b,
               tbaadm.utcd                  u,
               tbaadm.hth                   h
         where t.dth_init_sol_id = b.sol_id
           and t.user_part_tran_code = u.user_tran_code(+)
           and u.tran_code_type(+) = 'P'
           and t.acid =
               (select acid from tbaadm.gam where foracid = :NUBAN)
           AND t.pstd_flg = 'Y'
           and t.del_flg = 'N'
           AND trunc(t.tran_date) >= :FROM_DATE
           and trunc(t.tran_date) <= :TO_DATE
           and t.tran_date = h.tran_date(+)
           and t.tran_id = h.tran_id(+)) 

SELECT
 A.TRA_DATE,
       A.VAL_DATE,
       A.REFERENCE,
       A.DEBIT,
       A.CREDIT,
       to_char(nvl(b.opening_bal, 0) + nvl(A.RUNNING_BALANCE, 0),
               '9,999,999,999,999.99') RUNNING_BALANCE,
       A.ORIG_BRA,
       A.REMARKS,
       A.TRA_SEQ1,
       A.TRA_SEQ2,
       B.opening_bal,
       A.PSTD_DATE
  FROM ( ---PICK THE TRANSACTIONS
  SELECT DISTINCT replace(to_chAR( t.tran_date,
                                        'dd-Mon-yyyy'),
                                ',',
                                ',  ') tra_date,
                        replace(to_chAR(t.Value_date, 'dd-Mon-yyyy'),
                                ',',
                                ',  ') Val_date,
                        t.pstd_date,
                        '''' || t.ref_num ||
                        nvl(h.delivery_channel_id, t.module_id) as reference,
                        decode(t.part_tran_type,
                               'D',
                               to_char(t.tran_amt, '9,999,999,999,999.99'),
                               'C',
                               '') as Debit,
                        decode(t.part_tran_type,
                               'D',
                               '',
                               'C',
                               to_char(t.tran_amt, '9,999,999,999,999.99')) as Credit,
                        
                        to_char(t.acct_balance, '9,999,999,999,999.99') as Balance,
                        
                        sum(decode(part_tran_type,
                                   'C',
                                   tran_amt,
                                   'D',
                                   -tran_amt)) over(order by pstd_date,t.tran_id,t.part_tran_srl_num rows unbounded preceding
                        ) running_balance,
                        
                        b.sol_desc as Orig_bra,
                        (case
                          when user_part_tran_code in ('214', '1008') and
                               (UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE '%FEES%' OR
                               UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE '%CHARGE%' OR
                               UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE
                               '%RENEWAL%NAIRA%CARD%') then
                           NULL
                          ELSE
                           user_tran_code_desc
                        end) || ' ' || t.tran_particular || '' || t.tran_rmks || '' ||
                        t.tran_particular_2 as remarks,
                        t.tran_id tra_seq1,
                        t.part_tran_srl_num tra_seq2,
                        decode(t.part_tran_type,
                               'D',
                               to_char(t.acct_balance + t.tran_amt,
                                       '9,999,999,999,999.99'),
                               'C',
                               to_char(t.acct_balance - t.tran_amt,
                                       '9,999,999,999,999.99')) as OpeningBalance
          From CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
               tbaadm.sol                   b,
               tbaadm.utcd                  u,
               tbaadm.hth                   h
         where t.dth_init_sol_id = b.sol_id
           and t.user_part_tran_code = u.user_tran_code(+)
           and u.tran_code_type(+) = 'P'
           and t.acid =
               (select acid from tbaadm.gam where foracid = :NUBAN)
           AND t.pstd_flg = 'Y'
           and t.del_flg = 'N'
           AND trunc(t.tran_date) >= :FROM_DATE
           and trunc(t.tran_date) <= :TO_DATE
           and t.tran_date = h.tran_date(+)
           and t.tran_id = h.tran_id(+)
           ) A,
        
       -----GET THE OPENING BALANCE
       (SELECT (nvl(acct_balance, 0) +
               decode(t.part_tran_type, 'C', -tran_amt, 'D', tran_amt)) opening_bal
          From CUSTOM.CTD_DTD_ACLI_VIEW_ALL t, tbaadm.sol b, tbaadm.utcd u
         where t.dth_init_sol_id = b.sol_id
           and t.user_part_tran_code = u.user_tran_code(+)
           and u.tran_code_type(+) = 'P'
           and t.acid =
               (select acid from tbaadm.gam where foracid = :NUBAN)
           AND t.pstd_flg = 'Y'
           and t.del_flg = 'N'
           and trunc(t.tran_date) >= :FROM_DATE
           and trunc(t.tran_date) <= :TO_DATE
       
           
           AND t.pstd_date =
               (select min(t.pstd_date)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE)
         
         
                   
           and t.tran_id = 
               (select min(t.tran_id)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE
                   AND t.pstd_date =
               (select min(t.pstd_date)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE))
         
         
                
              and t.part_tran_srl_num = (select min(t.part_tran_srl_num)       
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE
              and t.tran_id = 
               (select min(t.tran_id)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE
                   AND t.pstd_date =
               (select min(t.pstd_date)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE)))
           
              ) b
 WHERE (SELECT MIN_TRA_DATE FROM MTD) < TO_DATE('14-10-2024', 'DD-MM-YYYY')----:FROM_DATE < TO_DATE('14-10-2024', 'DD-MM-YYYY') --DO NOT CHANGE THE RIGHT HAND SIDE DATE(CUTOVER DATE)

UNION ALL

----FOR TRANSACTIONS FROM CUTOVER DATE 14-10-2024 AND ABOVE (FINACLE ONLY TRANSACTIONS), 
---THIS SECOND QUERY WILL RUN (CHECK LAST CONDITION IN QUERY)

SELECT A.TRA_DATE,
       A.VAL_DATE,
       A.REFERENCE,
       A.DEBIT,
       A.CREDIT,
       to_char(nvl(b.OPENING_BALANCE, 0) + nvl(A.RUNNING_BALANCE, 0),
               '9,999,999,999,999.99') RUNNING_BALANCE,
       A.ORIG_BRA,
       A.REMARKS,
       A.TRA_SEQ1,
       A.TRA_SEQ2,
       B.OPENING_BALANCE,
       A.PSTD_DATE
  FROM (SELECT DISTINCT replace(to_chAR( t.tran_date,
                                        'dd-Mon-yyyy'),
                                ',',
                                ',  ') tra_date,
                        replace(to_chAR(t.Value_date, 'dd-Mon-yyyy'),
                                ',',
                                ',  ') Val_date,
                        t.pstd_date,
                        '''' || t.ref_num ||
                        nvl(h.delivery_channel_id, t.module_id) as reference,
                        decode(t.part_tran_type,
                               'D',
                               to_char(t.tran_amt, '9,999,999,999,999.99'),
                               'C',
                               '') as Debit,
                        decode(t.part_tran_type,
                               'D',
                               '',
                               'C',
                               to_char(t.tran_amt, '9,999,999,999,999.99')) as Credit,
                        
                        to_char(t.acct_balance, '9,999,999,999,999.99') as Balance,
                        
                        sum(decode(part_tran_type,
                                   'C',
                                   tran_amt,
                                   'D',
                                   -tran_amt)) over(order by pstd_date,t.tran_id,t.part_tran_srl_num rows unbounded preceding
                        ) running_balance,
                        
                        b.sol_desc as Orig_bra,
                        (case
                          when user_part_tran_code in ('214', '1008') and
                               (UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE '%FEES%' OR
                               UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE '%CHARGE%' OR
                               UPPER(T.tran_particular||t.tran_rmks|| t.tran_particular_2) LIKE
                               '%RENEWAL%NAIRA%CARD%') then
                           NULL
                          ELSE
                           user_tran_code_desc
                        end) || ' ' || t.tran_particular || '' || t.tran_rmks || '' ||
                        t.tran_particular_2 as remarks,
                        t.tran_id tra_seq1,
                        t.part_tran_srl_num tra_seq2,
                        decode(t.part_tran_type,
                               'D',
                               to_char(t.acct_balance + t.tran_amt,
                                       '9,999,999,999,999.99'),
                               'C',
                               to_char(t.acct_balance - t.tran_amt,
                                       '9,999,999,999,999.99')) as OpeningBalance
          From CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
               tbaadm.sol                   b,
               tbaadm.utcd                  u,
               tbaadm.hth                   h
         where t.dth_init_sol_id = b.sol_id
           and t.user_part_tran_code = u.user_tran_code(+)
           and u.tran_code_type(+) = 'P'
           and t.acid =
               (select acid from tbaadm.gam where foracid = :NUBAN)
           AND t.pstd_flg = 'Y'
           and t.del_flg = 'N'
           AND trunc(t.tran_date) >= :FROM_DATE
           and trunc(t.tran_date) <= :TO_DATE
           and t.tran_date = h.tran_date(+)
           and t.tran_id = h.tran_id(+)
           ) A,
       
       (SELECT NVL(TRAN_DATE_BAL - TRAN_DATE_TOT_TRAN, 0) OPENING_BALANCE
          FROM TBAADM.EAB A
         WHERE acid =
               (select acid from tbaadm.gam where foracid = :NUBAN)
           AND EOD_DATE =
               (select min(t.tran_date)
                  from CUSTOM.CTD_DTD_ACLI_VIEW_ALL t,
                       tbaadm.sol                   b,
                       tbaadm.utcd                  u
                 where t.dth_init_sol_id = b.sol_id
                   and t.user_part_tran_code = u.user_tran_code(+)
                   and u.tran_code_type(+) = 'P'
                   and t.acid = (select acid
                                   from tbaadm.gam
                                  where foracid = :NUBAN)
                   AND t.pstd_flg = 'Y'
                   and t.del_flg = 'N'
                   and trunc(t.tran_date) >= :FROM_DATE
                   and trunc(t.tran_date) <= :TO_DATE)) b

 WHERE (SELECT MIN_TRA_DATE FROM MTD) >= TO_DATE('14-10-2024', 'DD-MM-YYYY')---:FROM_DATE >= TO_DATE('14-10-2024', 'DD-MM-YYYY') --DO NOT CHANGE THE RIGHT HAND SIDE DATE(CUTOVER DATE)

 order by pstd_date,tra_seq1,tra_seq2