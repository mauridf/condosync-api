-- ============================================
-- CondoSync v2.0 - Migration V003
-- Registration flow: add tracking fields
-- ============================================

-- Adicionar coluna para rastrear qual User foi criado a partir do convite
ALTER TABLE public.unit_invitations
    ADD COLUMN user_id UUID REFERENCES public.users(id);

CREATE INDEX idx_invitations_user ON public.unit_invitations(user_id) WHERE user_id IS NOT NULL;
