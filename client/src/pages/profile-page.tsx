import { useEffect } from 'react';
import { useAppDispatch } from '@/hooks/redux';
import { fetchDiagnosisHistoryThunk, fetchPrescriptionsThunk, fetchMedicationRemindersThunk, fetchHealthMetricsThunk } from '@/features/profile/profile-slice';
import { ProfileHeader } from '@/features/profile/components/ProfileHeader';
import { HealthProgressChart } from '@/features/profile/components/HealthProgressChart';
import { MedicationReminders } from '@/features/profile/components/MedicationReminders';
import { CurrentPrescriptions } from '@/features/profile/components/CurrentPrescriptions';
import { DiagnosisHistory } from '@/features/profile/components/DiagnosisHistory';
import { AIProfileInsight } from '@/features/profile/components/AIProfileInsight';

export function ProfilePage() {
  const dispatch = useAppDispatch();

  useEffect(() => {
    dispatch(fetchDiagnosisHistoryThunk({ pageSize: 5 }));
    dispatch(fetchPrescriptionsThunk({ pageSize: 3 }));
    dispatch(fetchMedicationRemindersThunk({ pageSize: 5 }));
    dispatch(fetchHealthMetricsThunk({ pageSize: 14 })); // fetch a bit more for charting
  }, [dispatch]);

  return (
    <div className="pt-8 pb-24 px-6 md:px-8 max-w-7xl mx-auto animate-in fade-in zoom-in-95 duration-500">
      <ProfileHeader />
      
      <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
        <HealthProgressChart />
        <MedicationReminders />
        <CurrentPrescriptions />
        <DiagnosisHistory />
      </div>

      <AIProfileInsight />
    </div>
  );
}
